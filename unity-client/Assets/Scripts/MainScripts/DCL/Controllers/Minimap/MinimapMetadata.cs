using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapMetadata", menuName = "MinimapMetadata")]
public class MinimapMetadata : ScriptableObject
{
    public enum TileType
    {
        MyParcel = 0,
        MyParcelsOnSale = 1,
        MyEstates = 2,
        MyEstatesOnSale = 3,
        WithAccess = 4,
        District = 5,
        Contribution = 6,
        Roads = 7,
        Plaza = 8,
        Taken = 9,
        OnSale= 10,
        Unowned= 11, 
        Background = 12, 
        Loading = 13
    }

    //This must be replaced by standard Color type when explorer send the model in a proper way
    private static readonly Dictionary<TileType, string> TileColors = new Dictionary<TileType, string>()
    {
        { TileType.MyParcel, "#ff9990" },//Color on MktPlace #ff9990
        { TileType.MyParcelsOnSale, "#ff9990" },//Color on MktPlace #ec5159
        { TileType.MyEstates, "#ff9990" },//Color on MktPlace #ff9990
        { TileType.MyEstatesOnSale, "#ff9990" },//Color on MktPlace #ff4053
        { TileType.WithAccess, "#33303B" },//Color on MktPlace #ffbd33
        { TileType.District, "#5054D4" },//Color on MktPlace #4f57cc
        { TileType.Contribution, "#563db8" },
        { TileType.Roads, "#525D67" },//Color on MktPlace #706c79
        { TileType.Plaza, "#3FB86F" },//Color on MktPlace #7daa7b
        { TileType.Taken, "#33303B" },//Color on MktPlace #3c3a45
        { TileType.OnSale, "#33303B" },//Color on MktPlace #5ed0fa
        { TileType.Unowned, "#33303B" },//Color on MktPlace #09080A
        { TileType.Background, "#000000" },//Color on MktPlace #18141a
        { TileType.Loading, "#110e13" }
    };

    [Serializable]
    public class Tile
    {
        public Vector2Int position;
        public TileType tileType;
        public Color color;
        public string name;

        public Tile(Vector2Int position, int tileType, string name = "") : this(position, (TileType)tileType, name) { }
        public Tile(Vector2Int position, TileType tileType, string name = "")
        {
            this.position = position;
            this.tileType = tileType;
            this.name = name;

            //This must be replaced by standard Color type when explorer send the model in a proper way
            ColorUtility.TryParseHtmlString(TileColors[tileType], out color);
        }

    }

    [Serializable]
    public class Model
    {
        public Vector2Int bottomLeftCorner;
        public Vector2Int topRightCorner;
        public Tile[] tiles = new Tile[0];

        public int rowCount => (topRightCorner.y - bottomLeftCorner.y) + 1;
        public int colCount => (topRightCorner.x - bottomLeftCorner.x) + 1;

        public Model() { }

        public Model(Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
        {
            this.bottomLeftCorner = bottomLeftCorner;
            this.topRightCorner = topRightCorner;
            tiles = new Tile[rowCount * colCount];
        }

        public void AddTile(int x, int y, Tile tile)
        {
            int index = IndexFromCoordinates(x, y);

            if (index < 0 || index >= tiles.Length)
            {
                Debug.LogError($"Calculated Index: {index} for tile ({x},{y}) should be in the range [0,{tiles.Length})");
                return;
            }

            tiles[index] = tile;
        }

        public Tile GetTile(int x, int y)
        {
            int index = IndexFromCoordinates(x, y);


            if (index < 0 || index >= tiles.Length)
            {
                return null;
            }
            return tiles[IndexFromCoordinates(x, y)];
        }

        public int IndexFromCoordinates(int x, int y)
        {
            return ((x - bottomLeftCorner.x) * ((topRightCorner.y - bottomLeftCorner.y) + 1) + (y - bottomLeftCorner.y));
        }
    }

    public event Action<MinimapMetadata> OnChange = (x) => { };
    [SerializeField] private Model model;

    public void UpdateData(Model newModel)
    {
        model = newModel;
        OnChange(this);
    }

    public Tile GetTile(int x, int y) => model?.GetTile(x, y);

    private static MinimapMetadata minimapMetadata;
    public static MinimapMetadata GetMetadata()
    {
        if (minimapMetadata == null)
        {
            minimapMetadata = Resources.Load<MinimapMetadata>("ScriptableObjects/MinimapMetadata");
        }

        return minimapMetadata;
    }
}