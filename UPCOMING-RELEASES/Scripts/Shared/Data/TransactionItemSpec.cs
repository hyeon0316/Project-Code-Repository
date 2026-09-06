[System.Serializable]
public class TransactionItemSpec
{
    public int ID;
    public int Amount;

    public TransactionItemSpec()
    {

    }
    public TransactionItemSpec(int id, int amount)
    {
        ID = id;
        Amount = amount;
    }
}