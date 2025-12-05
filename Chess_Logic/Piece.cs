namespace Chess_Logic
{
    public class Piece
    {
        public PieceType PieceType { get; internal set; }
        public readonly PieceColor PieceColor;
        internal bool CanEnPassantDown = false;
        internal bool CanEnPassantUp = false;

        public Piece(PieceType pieceType, PieceColor pieceColor)
        {
            this.PieceType = pieceType;
            this.PieceColor = pieceColor;
        }

        internal List<(int, int)> GetLegalMoves(Game game, int row, int col)
        {
            Game gameClone = game.CloneGame();

            List<(int, int)> moves = [];

            (int rowMove, int colMove)[] directions = GetDirections();

            foreach (var direction in directions)
            {
                int moveRow = row + direction.rowMove;
                int moveCol = col + direction.colMove;

                if (PieceType is PieceType.Queen or PieceType.Rook or PieceType.Bishop)
                {
                    bool keepSearching = true;
                    do
                    {
                        bool leavesKingInCheck = DoesMoveLeaveKingInCheck(gameClone, row, col, moveRow, moveCol);

                        if (Game.IsValidPosition(moveRow, moveCol) || leavesKingInCheck)
                        {
                            keepSearching = true;
                        }
                        if (CanMoveToCell(game, moveRow, moveCol, out keepSearching) && !leavesKingInCheck)
                        {
                            moves.Add((moveRow, moveCol));
                        }

                        moveRow += direction.rowMove;
                        moveCol += direction.colMove;
                    }
                    while (keepSearching);
                }
                else if (PieceType is PieceType.King or PieceType.Knight)
                {
                    if (DoesMoveLeaveKingInCheck(gameClone, row, col, moveRow, moveCol))
                        continue;

                    if (CanMoveToCell(game, moveRow, moveCol, out _))
                    {
                        moves.Add((moveRow, moveCol));
                    }
                }
                else
                {
                    bool moveIsStraightLine = direction.colMove == 0;
                    if (CanPawnMoveToCell(game, moveRow, moveCol, moveIsStraightLine))
                    {
                        if (!DoesMoveLeaveKingInCheck(gameClone, row, col, moveRow, moveCol))
                            moves.Add((moveRow, moveCol));

                        moveRow += direction.rowMove;
                        if (moveIsStraightLine
                         && IsPawnOnStartingPosition(row)
                         && CanPawnMoveToCell(game, moveRow, moveCol, moveIsStraightLine)
                         && !DoesMoveLeaveKingInCheck(gameClone, row, col, moveRow, moveCol))
                        {
                            moves.Add((moveRow, moveCol));
                        }
                    }
                    else if (!moveIsStraightLine && !DoesMoveLeaveKingInCheck(gameClone, row, col, moveRow, moveCol))
                    {
                        if (int.IsNegative(direction.colMove) && CanEnPassantUp)
                            moves.Add((moveRow, moveCol));
                        else if (!int.IsNegative(direction.colMove) && CanEnPassantDown)
                            moves.Add((moveRow, moveCol));
                    }
                }
            }

            if (PieceType == PieceType.King)
            {
                if (CanKingLongCastle(game))
                {
                    moves.Add((row, col - 2));
                }
                if (CanKingShortCastle(game))
                {
                    moves.Add((row, col + 2));
                }
            }

            return moves;
        }

        private bool DoesMoveLeaveKingInCheck(Game gameClone, int startingRow, int startingCol, int moveRow, int moveCol)
        {
            if (Game.IsValidPosition(moveRow, moveCol))
            {
                Piece? oldPiece = gameClone.Board[moveRow, moveCol];
                gameClone.SimulatePieceMove(startingRow, startingCol, moveRow, moveCol);
                bool leavesKingInCheck = gameClone.IsKingInCheck(PieceColor);
                gameClone.SimulatePieceMove(moveRow, moveCol, startingRow, startingCol);
                gameClone.Board[moveRow, moveCol] = oldPiece;
                return leavesKingInCheck;
            }

            return false;
        }

        private bool IsPawnOnStartingPosition(int row)
        {
            if (PieceType != PieceType.Pawn)
                throw new ArgumentException("Not a pawn!");

            if (PieceColor == PieceColor.White)
                return row == 1;
            else
                return row == 6;
        }

        private bool CanKingShortCastle(Game game) // MAYBE CHANGE BOTH IF DECIDE TO ADD FISCHER RANDOM CHESS MODE
        {
            int kingRow = PieceColor == PieceColor.White ? 0 : 7;

            if (!game.HasKingOrUpRookMoved(PieceColor))
            {
                if (!game.IsCellOccupied(kingRow, 5) && !game.IsCellOccupied(kingRow, 6)
                 && !game.IsCellAttacked(PieceColor, kingRow, 5) && !game.IsCellAttacked(PieceColor, kingRow, 6))
                    return true;
            }

            return false;
        }

        private bool CanKingLongCastle(Game game)
        {
            int kingRow = PieceColor == PieceColor.White ? 0 : 7;

            if (!game.HasKingOrDownRookMoved(PieceColor) && !game.IsKingInCheck(PieceColor))
            {
                if (!game.IsCellOccupied(kingRow, 3) && !game.IsCellOccupied(kingRow, 2) && !game.IsCellOccupied(kingRow, 1)
                 && !game.IsCellAttacked(PieceColor, kingRow, 3) && !game.IsCellAttacked(PieceColor, kingRow, 2) && !game.IsCellAttacked(PieceColor, kingRow, 1))
                    return true;
            }

            return false;
        }

        private bool CanMoveToCell(Game game, int row, int col, out bool keepSearching)
        {
            if (Game.IsValidPosition(row, col))
            {
                if (!game.IsCellOccupied(row, col))
                {
                    keepSearching = true;
                    return true;
                }
                else if (game.IsCellOccupiedByEnemy(PieceColor, row, col))
                {
                    keepSearching = false;
                    return true;
                }
            }

            keepSearching = false;
            return false;
        }

        private bool CanPawnMoveToCell(Game game, int row, int col, bool moveIsStraightLine)
        {
            if (Game.IsValidPosition(row, col))
            {
                if (moveIsStraightLine)
                {
                    return !game.IsCellOccupied(row, col);
                }
                else
                {
                    return game.IsCellOccupiedByEnemy(PieceColor, row, col);
                }
            }

            return false;
        }

        internal static readonly (int, int)[] StraightMoves =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1)
        ];

        internal static readonly (int, int)[] DiagonalMoves =
        [
             (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        internal static readonly (int, int)[] KnightMoves =
        [
             (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2)
        ];

        internal static (int, int)[] AllDirections
        {
            get => StraightMoves.Concat(DiagonalMoves).ToArray();
        }

        internal (int, int)[] GetDirections()
        {
            switch (PieceType)
            {
                case PieceType.Pawn:
                    return
                    [
                        PieceColor == PieceColor.White ? (1, 0) : (-1, 0),
                        PieceColor == PieceColor.White ? (1, 1) : (-1, 1),
                        PieceColor == PieceColor.White ? (1, -1) : (-1, -1),
                    ];
                case PieceType.Knight:
                    return KnightMoves;
                case PieceType.Bishop:
                    return DiagonalMoves;
                case PieceType.Rook:
                    return StraightMoves;
                case PieceType.Queen:
                case PieceType.King:
                    return AllDirections;

                default:
                    throw new ArgumentException();
            };
        }
    }
}
