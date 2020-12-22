using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class BoardControler : MonoBehaviour
{
    public static BoardControler Instantiate;
    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;

    private Tile oldSelected;

    public UnityEvent OnEat;

    private bool isShift = false;
    private bool isSearchEmpty = false;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private bool isFindMatch = false;
    public void SetValue(Tile[,]tileArray,int xSize,int ySize, List<Sprite> tileSprite)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileArray = tileArray;
        this.tileSprite = tileSprite;
    }
    private void Awake()
    {
        Instantiate = this;
    }
    // Start is called before the first frame update
  

    // Update is called once per frame
    void Update()
    {
        if (isSearchEmpty)
        {
            SearchEmptyTile();
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray!=false)
            {
                CheckSelected(ray.collider.GetComponent<Tile>());
            }
        }
    }
     private void SelectTile(Tile tile)
    {
        tile.IsSelected = true;
        tile.SpriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelected = tile;
    }
    private void DeselectTile(Tile tile)
    {
        tile.IsSelected = false;
        tile.SpriteRenderer.color = new Color(1, 1, 1);
        oldSelected = null;
    }
    private void CheckSelected(Tile tile)
    {
        if (tile.IsEmpty|| isShift)
        {
            return;
        }
        if (tile.IsSelected)
        {
            DeselectTile(tile);

        }
        else
        {
            // первое выделение
            if (!tile.IsSelected && oldSelected==null)
            {
                SelectTile(tile);
            }
            //попытка выброть другой таил
            else
            {
                // если 2 выбраный сосед предыдушего
                if (AdjacentTile().Contains(tile))
                {
                 SwaptWoTile(tile);
                    FindAllMatch(tile);
                DeselectTile(oldSelected);
                }
                else
                {
                    DeselectTile(oldSelected);
                    SelectTile(tile);
                }
               
            }
        }
    }
    #region(Поиск совподений, удаление спрайтов ,движение тайтлов)
    private List<Tile>FindMatch(Tile tile,Vector2 dir)
    {
        List<Tile> cashFindTile = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null && hit.collider.gameObject.GetComponent<Tile>().SpriteRenderer.sprite == tile.SpriteRenderer.sprite) 
        {
            cashFindTile.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir);
        }
        return cashFindTile;
    }
    private void DeleteSpriet(Tile tile , Vector2[] dirArray)
    {
        List<Tile> cashFindsprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindsprite.AddRange(FindMatch(tile, dirArray[i]));
        }
        if (cashFindsprite.Count>=2)
        {
            for (int i = 0; i < cashFindsprite.Count; i++)
            {
            cashFindsprite[i].SpriteRenderer.sprite = null;
             }
            isFindMatch = true;
        }
       
    }
    private void FindAllMatch(Tile tile)
    {
        if (tile.IsEmpty)
        {
            return;
        }
        DeleteSpriet(tile, new Vector2[2] { Vector2.up, Vector2.down });

        DeleteSpriet(tile, new Vector2[2] { Vector2.left, Vector2.right });
        if (isFindMatch)
        {
            isFindMatch = false;
            tile.SpriteRenderer.sprite = null;
            isSearchEmpty = true;
        }
    }
    #endregion
    private void SwaptWoTile(Tile tile)
    {
        if (oldSelected.SpriteRenderer.sprite==tile.SpriteRenderer.sprite)
        {
            return;
        }
        Sprite cashSprite = oldSelected.SpriteRenderer.sprite;
        oldSelected.SpriteRenderer.sprite = tile.SpriteRenderer.sprite;
        tile.SpriteRenderer.sprite = cashSprite;
        Ui.instance.Move(1);
    }
    private List<Tile> AdjacentTile()
    {
        List<Tile> cashTiles = new List<Tile>();
        for (int i = 0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelected.transform.position, dirRay[i]);
            if (hit.collider!=null)
            {
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
        }
        return cashTiles;
    }
    #region(Поиск пустого тайла , Сдвиг тайла в низ , Установить изоброжение , Выбрать новое изображение)
    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tileArray[x,y].IsEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }
                if (x==xSize && y==ySize-1)
                {
                    isSearchEmpty = false;
                }
            }
        }
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                FindAllMatch(tileArray[x,y]);
            }
        }
    }
    private void ShiftTileDown(int xPos,int yPos)
    {
        isShift = true;
        List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
        int count = 0;
        for (int y = yPos; y < ySize; y++)
        {
            Tile tile = tileArray[xPos, y];
            if (tile.IsEmpty)
            {
                count++;
                Ui.instance.Score(10);
                if (OnEat != null)
                {
                    OnEat.Invoke();
                }
                /////////////////////////////////////////////////////////////////////////////
            }
            cashRenderer.Add(tile.SpriteRenderer);

        }
        for (int i = 0; i < count; i++)
        {
         SetNewSprite(xPos, cashRenderer);
        }
        
        isShift = false;
    }
    private void SetNewSprite(int xPos,List<SpriteRenderer> renderer)
    {
        for (int y = 0; y < renderer.Count-1; y++)
        {
            renderer[y].sprite = renderer[y + 1].sprite;
            renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1);
        }
    }
    private Sprite GetNewSprite(int xPos, int yPos)
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite);
        if (xPos>0)
        {
            cashSprite.Remove(tileArray[xPos - 1, yPos].SpriteRenderer.sprite);
        }
        if (xPos < xSize-1)
        {
            cashSprite.Remove(tileArray[xPos + 1, yPos].SpriteRenderer.sprite);
        }
        if (xPos > 0)
        {
            cashSprite.Remove(tileArray[xPos,yPos - 1].SpriteRenderer.sprite);
        }
        return cashSprite[Random.Range(0, cashSprite.Count)];
    }
    #endregion
}
