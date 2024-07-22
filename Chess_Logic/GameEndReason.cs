namespace Chess_Logic
{
    public enum GameEndReason
    {
        StillNotOver,
        Checkmate,
        Resignation,
        OnTime,
        DrawByRepetition,
        DrawByStalemate,
        DrawBy50MovesRule,
        DrawByInsufficientMaterial,
        DrawByAgreement,
        OnTimeAgainstInsufficientMaterial
    }
}
