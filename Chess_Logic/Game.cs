namespace Chess_Logic
{
    public class Game
    {
        public PieceColor Turn { get; private set; } = PieceColor.White;
        public Piece?[,] Board { get; private set; } = new Piece?[8, 8];

        public Game()
        {
            SetUpPieces(0, PieceColor.White);
            SetUpPawns(1, PieceColor.White);
            SetUpPawns(6, PieceColor.Black);
            SetUpPieces(7, PieceColor.Black);
        }

        internal static bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        public List<(int, int)> GetLegalMoves(int row, int col)
        {
            if (!IsCellOccuped(row, col))
                throw new ArgumentException("No piece on the specified position.");

            return Board[row, col]!.GetLegalMoves(Board, row, col);
        }

        public void ChangeTurn()
        {
            if (Turn == PieceColor.White)
                Turn = PieceColor.Black;
            else
                Turn = PieceColor.White;
        }

        public bool IsCellOccuped(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return false;

            return Board[row, col] != null;
        }

        public void MovePiece(int oldRow, int oldCol, int NewRow, int newCol)
        {
            if (!IsCellOccuped(oldRow, oldCol))
                throw new ArgumentException("No piece on the specified position.");

            Piece piece = Board[oldRow, oldCol]!;
            Board[oldRow, oldCol] = null;
            Board[NewRow, newCol] = piece;
        }

        public PieceColor GetPieceColor(int row, int col)
        {
            if (!IsCellOccuped(row, col))
                throw new ArgumentException("No piece on the specified position.");

            return Board[row, col]!.PieceColor;
        }

        public string GetPieceImg(int row, int col)
        {
            if (!IsCellOccuped(row, col))
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
