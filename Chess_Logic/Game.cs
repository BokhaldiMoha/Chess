namespace Chess_Logic
{
    public class Game
    {
        public PieceColor Turn { get; private set; }
        public GameState GameState { get; private set; }
        public GameEndReason GameEndReason { get; private set; }
        public Piece?[,] Board { get; internal set; }
        public (int row, int col) WhiteKingPosition { get; private set; }
        public (int row, int col) BlackKingPosition { get; private set; }
        public bool CanPlayerClaim50MovesRule { get; private set; }

        public event Action? GameStateChanged;
        public event Action? CanClaim50MovesRuleChanged;
        public event Action<PieceColor>? KingInCheckStateChanged;

        private List<string> RepeatablePositions;

        public Game()
        {
            GameState = GameState.Playing;
            GameEndReason = GameEndReason.StillNotOver;

            Turn = PieceColor.White;
            Board = new Piece?[8, 8];

            WhiteKingPosition = (0, 4);
            BlackKingPosition = (7, 4);

            SetUpPieces(0, PieceColor.White);
            SetUpPawns(1, PieceColor.White);
            SetUpPawns(6, PieceColor.Black);
            SetUpPieces(7, PieceColor.Black);

            CanPlayerClaim50MovesRule = false;
            RepeatablePositions = [StringifyBoard()];
        }

        public List<(int, int)> GetLegalMoves(int row, int col)
        {
            if (!IsCellOccupied(row, col))
                throw new ArgumentException("No piece on the specified position.");

            return Board[row, col]!.GetLegalMoves(this, row, col);
        }

        public void ChangeTurn()
        {
            if (Turn == PieceColor.White)
                Turn = PieceColor.Black;
            else
                Turn = PieceColor.White;
        }

        public bool IsKingInCheck(PieceColor allyColor)
        {
            return allyColor == PieceColor.White
                ? IsCellAttacked(allyColor, WhiteKingPosition.row, WhiteKingPosition.col)
                : IsCellAttacked(allyColor, BlackKingPosition.row, BlackKingPosition.col);
        }

        public bool IsCellOccupied(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return false;

            return Board[row, col] != null;
        }

        public bool IsCellAttacked(PieceColor allyColor, int row, int col)
        {
            return IsCellAttackedVertically(allyColor, row, col)
                || IsCellAttackedHorizontally(allyColor, row, col)
                || IsCellAttackedByKing(allyColor, row, col)
                || IsCellAttackedByKnight(allyColor, row, col)
                || IsCellAttackedByPawn(allyColor, row, col);
        }

        public void MovePiece(int oldRow, int oldCol, int newRow, int newCol)
        {
            if (!IsCellOccupied(oldRow, oldCol))
                throw new ArgumentException("No piece on the specified position.");

            Piece piece = Board[oldRow, oldCol]!;

            if (MovedPieces.ContainsKey((piece.PieceColor, piece.PieceType, oldCol)))
            {
                MovedPieces[(piece.PieceColor, piece.PieceType, oldCol)] = true;
            }

            if (piece.PieceType == PieceType.King)
            {
                if (oldCol - newCol == -2)
                {
                    Board[oldRow, 5] = Board[oldRow, 7];
                    Board[oldRow, 7] = null;
                }
                else if (oldCol - newCol == 2)
                {
                    Board[oldRow, 3] = Board[oldRow, 0];
                    Board[oldRow, 0] = null;
                }

                if (piece.PieceColor == PieceColor.White)
                    WhiteKingPosition = (newRow, newCol);
                else
                    BlackKingPosition = (newRow, newCol);
            }
            if (piece.PieceType == PieceType.Pawn && Math.Abs(oldRow - newRow) == 2)
            {
                if (IsCellOccupiedByEnemy(piece.PieceColor, newRow, newCol - 1) && Board[newRow, newCol - 1]!.PieceType == PieceType.Pawn)
                {
                    Board[newRow, newCol - 1]!.CanEnPassantDown = true;
                }
                if (IsCellOccupiedByEnemy(piece.PieceColor, newRow, newCol + 1) && Board[newRow, newCol + 1]!.PieceType == PieceType.Pawn)
                {
                    Board[newRow, newCol + 1]!.CanEnPassantUp = true;
                }
            }
            if (piece.PieceType == PieceType.Pawn && (piece.CanEnPassantDown || piece.CanEnPassantUp))
            {
                if (Math.Abs(oldCol - newCol) == 1 && Board[newRow, newCol] == null)
                {
                    Board[newRow + (piece.PieceColor == PieceColor.White ? -1 : 1), newCol] = null;
                }
            }

            if (piece.PieceType == PieceType.Pawn || Board[newRow, newCol] != null)
                RepeatablePositions.Clear();

            if (piece.PieceType == PieceType.Pawn)
                if ((piece.PieceColor == PieceColor.White && newRow == 7) || (piece.PieceColor == PieceColor.Black && newRow == 0))
                    piece.PieceType = PieceType.Queen;

            piece.CanEnPassantDown = false;
            piece.CanEnPassantUp = false;
            Board[oldRow, oldCol] = null;
            Board[newRow, newCol] = piece;

            PieceColor enemyColor = piece.PieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

            if (!DoesPlayerHaveLegalMoves(enemyColor))
            {
                if (IsKingInCheck(enemyColor))
                {
                    if (enemyColor == PieceColor.White)
                        GameState = GameState.BlackWins;
                    else
                        GameState = GameState.WhiteWins;

                    GameEndReason = GameEndReason.Checkmate;
                }
                else
                {
                    GameState = GameState.Draw;
                    GameEndReason = GameEndReason.DrawByStalemate;
                }

                GameStateChanged?.Invoke();
                return;
            }
            else if (IsKingInCheck(enemyColor))
            {
                KingInCheckStateChanged?.Invoke(enemyColor);
            }

            RepeatablePositions.Add(StringifyBoard());

            if (!IsThereSuficientMaterial())
            {
                GameState = GameState.Draw;
                GameEndReason = GameEndReason.DrawByInsufficientMaterial;

                GameStateChanged?.Invoke();
                return;
            }

            if (CanPlayerClaim50MovesRule != RepeatablePositions.Count >= 99)
            {
                CanPlayerClaim50MovesRule = RepeatablePositions.Count >= 99;
                CanClaim50MovesRuleChanged?.Invoke();
            }

            if (RepeatablePositions.Count >= 6)
            {
                if (RepeatablePositions.GroupBy(x => x).Any(x => x.Count() >= 3))
                {
                    GameState = GameState.Draw;
                    GameEndReason = GameEndReason.DrawByRepetition;

                    GameStateChanged?.Invoke();
                    return;
                }
            }

            ChangeTurn();
        }

        public void ClaimDrawBy50MovesRule()
        {
            if (!CanPlayerClaim50MovesRule)
                throw new InvalidOperationException("Can't claim 50 moves rule.");

            GameState = GameState.Draw;
            GameEndReason = GameEndReason.DrawBy50MovesRule;
            GameStateChanged?.Invoke();
        }

        public void SimulatePieceMove(int oldRow, int oldCol, int newRow, int newCol)
        {
            if (!IsCellOccupied(oldRow, oldCol))
                throw new ArgumentException("No piece on the specified position.");

            Piece piece = Board[oldRow, oldCol]!;

            if (piece.PieceType == PieceType.King)
            {
                if (piece.PieceColor == PieceColor.White) WhiteKingPosition = (newRow, newCol);
                else BlackKingPosition = (newRow, newCol);

                if (oldCol - newCol == -2)
                {
                    Board[oldRow, 5] = Board[oldRow, 7];
                    Board[oldRow, 7] = null;
                }
                else if (oldCol - newCol == 2)
                {
                    Board[oldRow, 3] = Board[oldRow, 0];
                    Board[oldRow, 0] = null;
                }
            }

            Board[oldRow, oldCol] = null;
            Board[newRow, newCol] = piece;
        }

        public PieceColor GetPieceColor(int row, int col)
        {
            if (!IsCellOccupied(row, col))
                throw new ArgumentException("No piece on the specified position.");

            return Board[row, col]!.PieceColor;
        }

        public string GetCellColor(int row, int col)
        {
            if ((row + col) % 2 == 0)
                return "whitesmoke";
            else
                return "green";
        }

        internal bool HasKingOrUpRookMoved(PieceColor pieceColor)
        {
            return MovedPieces[(pieceColor, PieceType.King, 4)] || MovedPieces[(pieceColor, PieceType.Rook, 7)];
        }

        internal bool HasKingOrDownRookMoved(PieceColor pieceColor)
        {
            return MovedPieces[(pieceColor, PieceType.King, 4)] || MovedPieces[(pieceColor, PieceType.Rook, 0)];
        }

        internal bool DoesPlayerHaveLegalMoves(PieceColor pieceColor)
        {
            bool hasLegalMoves = false;

            if (pieceColor == PieceColor.White)
                hasLegalMoves = Board[WhiteKingPosition.row, WhiteKingPosition.col]!.GetLegalMoves(this, WhiteKingPosition.row, WhiteKingPosition.col).Count != 0;
            else
                hasLegalMoves = Board[BlackKingPosition.row, BlackKingPosition.col]!.GetLegalMoves(this, BlackKingPosition.row, BlackKingPosition.col).Count != 0;

            for (int i = 0; i < 8 && !hasLegalMoves; i++)
            {
                for (int j = 0; j < 8 && !hasLegalMoves; j++)
                {
                    if (IsCellOccupiedByAlly(pieceColor, i, j))
                        hasLegalMoves = Board[i, j]!.GetLegalMoves(this, i, j).Count != 0;

                }
            }

            return hasLegalMoves;
        }

        internal static bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        internal bool IsCellOccupiedByEnemy(PieceColor allyColor, int row, int col)
        {
            return IsCellOccupied(row, col) && Board[row, col]!.PieceColor != allyColor;
        }

        internal bool IsCellOccupiedByAlly(PieceColor allyColor, int row, int col)
        {
            return IsCellOccupied(row, col) && Board[row, col]!.PieceColor == allyColor;
        }

        internal Game CloneGame()
        {
            Game game = new Game();
            game.Board = (Piece?[,])this.Board.Clone();
            game.MovedPieces = new Dictionary<(PieceColor, PieceType, int), bool>(this.MovedPieces);
            game.BlackKingPosition = this.BlackKingPosition;
            game.WhiteKingPosition = this.WhiteKingPosition;
            game.Turn = this.Turn;
            return game;
        }

        private bool IsCellAttackedVertically(PieceColor allyColor, int row, int col)
        {
            foreach ((int moveRow, int moveCol) move in Piece.StraightMoves)
            {
                int newRow = row + move.moveRow;
                int newCol = col + move.moveCol;

                while (IsValidPosition(newRow, newCol))
                {
                    if (IsCellOccupied(newRow, newCol))
                    {
                        if (IsCellOccupiedByEnemy(allyColor, newRow, newCol)
                         && Board[newRow, newCol]!.PieceType is PieceType.Queen or PieceType.Rook)
                            return true;

                        break;
                    }

                    newRow += move.moveRow;
                    newCol += move.moveCol;
                }
            }

            return false;
        }

        private bool IsCellAttackedHorizontally(PieceColor allyColor, int row, int col)
        {
            foreach ((int moveRow, int moveCol) move in Piece.DiagonalMoves)
            {
                int newRow = row + move.moveRow;
                int newCol = col + move.moveCol;

                while (IsValidPosition(newRow, newCol))
                {
                    if (IsCellOccupied(newRow, newCol))
                    {
                        if (IsCellOccupiedByEnemy(allyColor, newRow, newCol)
                         && Board[newRow, newCol]!.PieceType is PieceType.Queen or PieceType.Bishop)
                            return true;

                        break;
                    }

                    newRow += move.moveRow;
                    newCol += move.moveCol;
                }
            }

            return false;
        }

        private string StringifyBoard()
        {
            string result = "";

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Board[i, j] != null)
                    {
                        char pieceChar = ' ';

                        switch (Board[i, j]!.PieceType)
                        {
                            case PieceType.King:
                                pieceChar = 'k';
                                break;
                            case PieceType.Queen:
                                pieceChar = 'q';
                                break;
                            case PieceType.Rook:
                                pieceChar = 'r';
                                break;
                            case PieceType.Bishop:
                                pieceChar = 'b';
                                break;
                            case PieceType.Knight:
                                pieceChar = 'n';
                                break;
                            case PieceType.Pawn:
                                pieceChar = 'p';
                                break;
                        }

                        if (Board[i, j]!.PieceColor == PieceColor.Black)
                            pieceChar = Char.ToUpper(pieceChar);

                        result += pieceChar;
                    }
                    else
                    {
                        result += " ";
                    }
                }
            }

            return result;
        }

        private bool IsCellAttackedByKnight(PieceColor allyColor, int row, int col)
        {
            foreach ((int moveRow, int moveCol) move in Piece.KnightMoves)
            {
                int newRow = row + move.moveRow;
                int newCol = col + move.moveCol;
                if (IsCellOccupiedByEnemy(allyColor, newRow, newCol) &&
                    Board[newRow, newCol]!.PieceType == PieceType.Knight)
                    return true;
            }

            return false;
        }

        private bool IsCellAttackedByKing(PieceColor allyColor, int row, int col)
        {
            foreach ((int moveRow, int moveCol) move in Piece.AllDirections)
            {
                int newRow = row + move.moveRow;
                int newCol = col + move.moveCol;
                if (IsCellOccupiedByEnemy(allyColor, newRow, newCol) &&
                    Board[newRow, newCol]!.PieceType == PieceType.King)
                    return true;
            }

            return false;
        }

        private bool IsCellAttackedByPawn(PieceColor allyColor, int row, int col)
        {
            int moveRow = allyColor == PieceColor.White ? row + 1 : row - 1;

            return (IsCellOccupiedByEnemy(allyColor, moveRow, col + 1) && Board[moveRow, col + 1]!.PieceType == PieceType.Pawn)
                || (IsCellOccupiedByEnemy(allyColor, moveRow, col - 1) && Board[moveRow, col - 1]!.PieceType == PieceType.Pawn);
        }

        private bool IsThereSuficientMaterial()
        {
            string sBoard = StringifyBoard();
            return sBoard.Contains('p', StringComparison.InvariantCultureIgnoreCase) || sBoard.GroupBy(p => p).Any(p => char.ToLower(p.Key) != 'p' && p.Count() > 1);
        }

        private void SetUpPieces(int row, PieceColor pieceColor)
        {
            Board[row, 0] = new(PieceType.Rook, pieceColor);
            Board[row, 1] = new(PieceType.Knight, pieceColor);
            Board[row, 2] = new(PieceType.Bishop, pieceColor);
            Board[row, 3] = new(PieceType.Queen, pieceColor);
            Board[row, 4] = new(PieceType.King, pieceColor);
            Board[row, 5] = new(PieceType.Bishop, pieceColor);
            Board[row, 6] = new(PieceType.Knight, pieceColor);
            Board[row, 7] = new(PieceType.Rook, pieceColor);
        }

        private void SetUpPawns(int row, PieceColor pieceColor)
        {
            for (int col = 0; col < 8; col++)
            {
                Board[row, col] = new(PieceType.Pawn, pieceColor);
            }
        }

        internal Dictionary<(PieceColor, PieceType, int), bool> MovedPieces = new()
        {
            { (PieceColor.White, PieceType.King, 4), false },
            { (PieceColor.Black, PieceType.King, 4), false },
            { (PieceColor.White, PieceType.Rook, 0), false },
            { (PieceColor.White, PieceType.Rook, 7), false },
            { (PieceColor.Black, PieceType.Rook, 0), false },
            { (PieceColor.Black, PieceType.Rook, 7), false },
        };
    }
}