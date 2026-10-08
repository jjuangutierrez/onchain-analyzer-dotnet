using OnChainAnalyzer.Domain.ValueObjects;

namespace OnChainAnalyzer.Domain.Entities;

public record Utxo
{
    public string TransactionId { get; }
    public int OutputIndex { get; }
    public BitcoinAmount Amount { get; }
    public int? BlockHeight { get; }
    public bool IsConfirmed => BlockHeight.HasValue;
    public DateTime? BlockDate { get; }
    public int? AgeInDays => BlockDate.HasValue ? (int)(DateTime.UtcNow - BlockDate.Value).TotalDays : null;

    
    public Utxo(string transactionId, int outputIndex, BitcoinAmount amount, int? blockHeight, DateTime? blockDate)
    {
        ArgumentException.ThrowIfNullOrEmpty(transactionId);
        ArgumentNullException.ThrowIfNull(amount);
        
        var isTransactionValid = transactionId.Length == 64 && transactionId.All(c => Uri.IsHexDigit(c));

        if  (!isTransactionValid)
            throw new ArgumentException($"Invalid transaction ID: {transactionId}", nameof(transactionId));
        
        if (outputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(outputIndex), "Output index must be greater than or equal to zero.");
        
        if (blockHeight < 0)
            throw new ArgumentOutOfRangeException(nameof(blockHeight), "Block height must be greater than or equal to zero.");

        if (blockDate.HasValue && blockDate.Value < new DateTime(2009, 1, 3))
        {
            throw new ArgumentOutOfRangeException(nameof(blockDate), "Block date must be grater than January 3 2009");
        }
        
        TransactionId = transactionId;
        OutputIndex = outputIndex;
        Amount = amount;
        BlockHeight = blockHeight;
        BlockDate = blockDate;
    }
    
}