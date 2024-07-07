namespace Chess_Logic
{
    public enum GameEndReason
    {
        Checkmate,
        Resignation,
        OnTime,
        DrawByRepetition,
        DrawByStalemate,
        DrawBy50MovesRule,
        DrawByInsufficientMaterial,
        DrawByAgreement
    }
}
