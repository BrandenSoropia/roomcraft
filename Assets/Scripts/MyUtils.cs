using UnityEngine;

public static class MyUtils
{
    // Assumes first 3 sections of furniture GOs are underscored
    public static string GetFurnitureGOBaseName(string goName)
    {
        string[] namePieces = goName.Split(new[] { "_" }, System.StringSplitOptions.None);

        return $"{namePieces[0]}_{namePieces[1]}_{namePieces[2]}";

    }
}
