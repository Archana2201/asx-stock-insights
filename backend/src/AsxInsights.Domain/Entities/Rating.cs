namespace AsxInsights.Domain.Entities;

/// <summary>Overall quality bucket for a stock. This is a screening label, not advice.</summary>
public enum Rating
{
    Weak = 0,
    BelowAverage = 1,
    Neutral = 2,
    Favourable = 3,
    Strong = 4
}
