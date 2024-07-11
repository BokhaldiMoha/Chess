namespace Chess_Logic
{
    public class Game
    {
        public PieceColor Turn { get; private set; } = PieceColor.White;
        public Piece?[,] Board { get; internal set; } = new Piece?[8, 8];

        public (int row, int col) WhiteKingPosition = (0, 4);
        public (int row, int col) BlackKingPosition = (7, 4);

        public Game()
        {
            SetUpPieces(0, PieceColor.White);
            SetUpPawns(1, PieceColor.White);
            SetUpPawns(6, PieceColor.Black);
            SetUpPieces(7, PieceColor.Black);
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

            piece.CanEnPassantDown = false;
            piece.CanEnPassantUp = false;
            Board[oldRow, oldCol] = null;
            Board[newRow, newCol] = piece;
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

        public string GetPieceImg(int row, int col)
        {
            if (!IsCellOccupied(row, col))
                throw new ArgumentException("No piece on the specified position.");

            return Board[row, col]!.GetImg();
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

        internal static bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        internal bool IsCellOccupiedByEnemy(PieceColor allyColor, int row, int col)
        {
            return IsCellOccupied(row, col) && Board[row, col]!.PieceColor != allyColor;
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

        internal Dictionary<(PieceColor, PieceType, int), bool> MovedPieces = new Dictionary<(PieceColor, PieceType, int), bool>()
        {
            { (PieceColor.White, PieceType.King, 4), false },
            { (PieceColor.Black, PieceType.King, 4), false },
            { (PieceColor.White, PieceType.Rook, 0), false },
            { (PieceColor.White, PieceType.Rook, 7), false },
            { (PieceColor.Black, PieceType.Rook, 0), false },
            { (PieceColor.Black, PieceType.Rook, 7), false },
        };
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
    }
}