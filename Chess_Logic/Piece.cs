using System.Drawing;

namespace Chess_Logic
{
    public class Piece
    {
        internal readonly PieceType PieceType;
        internal readonly PieceColor PieceColor;
        internal bool CanEnPassantDown = false;
        internal bool CanEnPassantUp = false;

        public Piece(PieceType pieceType, PieceColor pieceColor)
        {
            this.PieceType = pieceType;
            this.PieceColor = pieceColor;
        }

        internal string GetImg()
        {
            string img = PieceColor == PieceColor.White ? "w" : "b";

            switch (PieceType)
            {
                case PieceType.King:
                    img += "k";
                    break;
                case PieceType.Queen:
                    img += "q";
                    break;
                case PieceType.Rook:
                    img += "r";
                    break;
                case PieceType.Bishop:
                    img += "b";
                    break;
                case PieceType.Knight:
                    img += "n";
                    break;
                case PieceType.Pawn:
                    img += "p";
                    break;
            }

            return img + ".png";
        }

        internal List<(int, int)> GetLegalMoves(Game game, int row, int col)
        {
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
                        if (CanMoveToCell(game, moveRow, moveCol, out keepSearching))
                        {
                            moves.Add((moveRow, moveCol));
                            moveRow += direction.rowMove;
                            moveCol += direction.colMove;
                        }
                    }
                    while (keepSearching);
                }
                else if (PieceType is PieceType.King or PieceType.Knight)
                {
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
                        moves.Add((moveRow, moveCol));

                        moveRow += direction.rowMove;
                        if (moveIsStraightLine && IsPawnOnStartingPosition(row) && CanPawnMoveToCell(game, moveRow, moveCol, moveIsStraightLine))
                        {
                            moves.Add((moveRow, moveCol));
                        }
                    }
                    else if (!moveIsStraightLine)
                    {
                        if (int.IsNegative(direction.colMove) && CanEnPassantDown)
                            moves.Add((moveRow, moveCol));
                        else if (!int.IsNegative(direction.colMove) && CanEnPassantUp)
                            moves.Add((moveRow, moveCol));
                    }
                }
            }

            return moves;
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

        private static readonly (int, int)[] StraightMoves =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1)
        ];

        private static readonly (int, int)[] DiagonalMoves =
        [
             (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        private (int, int)[] GetDirections()
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
                    return
                    [
                        (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2)
                    ];
                case PieceType.Bishop:
                    return DiagonalMoves;
                case PieceType.Rook:
                    return StraightMoves;
                case PieceType.Queen:
                case PieceType.King:
                    return StraightMoves.Concat(DiagonalMoves).ToArray();

                default:
                    throw new ArgumentException();
            };
        }
    }
}
