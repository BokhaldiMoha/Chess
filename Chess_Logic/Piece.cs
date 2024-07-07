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
    }
}
