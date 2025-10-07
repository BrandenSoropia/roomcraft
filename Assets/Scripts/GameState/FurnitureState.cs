public class FurnitureState
{
    public FurnitureDataSO data;
    public int assembledPieces;

    public FurnitureState(FurnitureDataSO data)
    {
        this.data = data;
        this.assembledPieces = 0;
    }

    public bool IsComplete => assembledPieces == data.numTotalPieces;
}