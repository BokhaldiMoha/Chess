using System.Drawing;

namespace Chess_Logic
{
    public class Piece
    {
        internal readonly PieceType PieceType;
        internal readonly PieceColor PieceColor;

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

        internal List<(int, int)> GetLegalMoves(Piece?[,] board, int row, int col)
        {
            List<(int row, int col)> moves = new List<(int row, int col)>();

            var directions = GetDirections();

            foreach (var direction in directions)
            {
                int newRow = row + direction.Item1;
                int newCol = col + direction.Item2;

                while (Game.IsValidPosition(newRow, newCol) && (board[newRow, newCol] == null || board[newRow, newCol]!.PieceColor != this.PieceColor))
                {
                    moves.Add((newRow, newCol));
                    if (board[newRow, newCol] != null && board[newRow, newCol].PieceColor != this.PieceColor)
                        break;
                    if (PieceType == PieceType.Knight || PieceType == PieceType.King || PieceType == PieceType.Pawn)
                        break;

                    newRow += direction.Item1;
                    newCol += direction.Item2;
                }
            }

            // Extra logic for pawns (moving forward and capturing)
            if (PieceType == PieceType.Pawn)
            {
                int direction = this.PieceColor == PieceColor.White ? 1 : -1;
                int startRow = this.PieceColor == PieceColor.White ? 1 : 6;

                // Forward move
                if (Game.IsValidPosition(row + direction, col) && board[row + direction, col] == null)
                {
                    moves.Add((row + direction, col));

                    // Double move from start
                    if (row == startRow && board[row + 2 * direction, col] == null)
                    {
                        moves.Add((row + 2 * direction, col));
                    }
                }
            }

            return moves;
        }

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
                    return
                    [
                        (1, 1), (1, -1), (-1, 1), (-1, -1)
                    ];
                case PieceType.Rook:
                    return
                    [
                        (1, 0), (-1, 0), (0, 1), (0, -1)
                    ];
                case PieceType.Queen:
                    return
                    [
                        (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1)
                    ];
                case PieceType.King:
                    return
                    [
                        (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1)
                    ];
                default:
                    throw new ArgumentException();
            };
        }
    }
}
