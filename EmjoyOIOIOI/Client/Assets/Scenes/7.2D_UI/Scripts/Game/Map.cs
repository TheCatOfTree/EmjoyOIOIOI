using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Map : MonoBehaviour
{
    public GameObject pItem;
    
    public int width;
    public int height;
    private Vector2 initPosition = new Vector2(300, 60);
    public ItemTable ac;
    private float offset = 110;
    public GameObject itemPrefab;
    public GameObject[,] items;
    public Item[,] Citems;
    private List<Item> waitRemoveItems;
    private List<Item> specialItems;
    public bool animationIsReady = false;

    public Vector3 touchPosition;
    public GameObject touchItem, forecastItem;
    public Item ItouchItem, IforecastItem;
    public Vector3 initItemPoint;
    private Vector3 forecastPosition;
    public Vector2 mouseVelocity;

    public int Initscore = 10;
    private int scoreOffset = 5;

    private void Start()
    {
        ac.map = transform.gameObject;
        items = new GameObject[height, width];
        Citems = new Item[height, width];
        waitRemoveItems = new List<Item>();
        specialItems = new List<Item>();
        CreatMap();
    }

    private void OnDestroy()
    {
        waitRemoveItems.Clear();
        specialItems.Clear();
        waitRemoveItems = null;
        specialItems = null;
    }


    private void Update()
    {
        IsMoving();

        if (touchItem && !forecastItem && mouseVelocity != Vector2.zero) //进入操作环节
        {
            UpdateTouchPosition();
        }

        if (touchItem && forecastItem)
        {
            ItemAnimation();
        }

        if (animationIsReady)
        {
            if (CheatMap())
            {
                ChangeIsRemove();
                Removed();
                Initscore += scoreOffset;
                Fall();
                GameManager.Instance.canSend = true;
            }
            else
            {
                Initscore = 10;
            }
        }
    }

    /// <summary>
    /// 计算即将交换的方块
    /// </summary>
    private void UpdateTouchPosition()
    {
        forecastPosition = mouseVelocity; //init+趋势
        if (forecastPosition.x >= 0)
        {
            if (forecastPosition.y >= 0)
            {
                if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) > 0 && ItouchItem.x != width - 1)
                {
                    forecastItem = items[ItouchItem.x + 1, ItouchItem.y];
                    //右边
                }
                else if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) <= 0 &&
                         ItouchItem.y != height - 1)
                {
                    forecastItem = items[ItouchItem.x, ItouchItem.y + 1];
                    //上边
                }
            }
            else
            {
                if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) > 0 && ItouchItem.x != width - 1)
                {
                    forecastItem = items[ItouchItem.x + 1, ItouchItem.y];
                    //右边
                }
                else if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) <= 0 && ItouchItem.y != 0)
                {
                    forecastItem = items[ItouchItem.x, ItouchItem.y - 1];
                    //下边
                }
            }
        }
        else
        {
            if (forecastPosition.y >= 0)
            {
                if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) > 0 && ItouchItem.x != 0)
                {
                    forecastItem = items[ItouchItem.x - 1, ItouchItem.y];
                    //左边
                }
                else if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) <= 0 &&
                         ItouchItem.y != height - 1)
                {
                    forecastItem = items[ItouchItem.x, ItouchItem.y + 1];
                    //上边
                }
            }
            else
            {
                if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) > 0 && ItouchItem.x != 0)
                {
                    forecastItem = items[ItouchItem.x - 1, ItouchItem.y];
                    //左边
                }
                else if (Mathf.Abs(forecastPosition.x) - Mathf.Abs(forecastPosition.y) <= 0 && ItouchItem.y != 0)
                {
                    forecastItem = items[ItouchItem.x, ItouchItem.y - 1];
                    //下边
                }
            }
        }

        if (forecastItem)
        {
            IforecastItem = forecastItem.GetComponent<Item>();
        }
    }

    /// <summary>
    /// 拖动方块时的动画表现
    /// </summary>
    private void ItemAnimation()
    {
        Vector3 itemWorldPosition =
            new Vector3(initPosition.x + ItouchItem.x * offset, initPosition.y + ItouchItem.y * offset, 0);
        Vector3 forecastWorldPosition = new Vector3(initPosition.x + IforecastItem.x * offset,
            initPosition.y + IforecastItem.y * offset, 0);

        float lerpCoefficient =
            (touchPosition - itemWorldPosition).sqrMagnitude / (forecastWorldPosition - itemWorldPosition).sqrMagnitude;
        float dot = Vector3.Dot(touchPosition - itemWorldPosition, (forecastWorldPosition - itemWorldPosition));
        if (dot > 0) //单向移动
        {
            touchItem.transform.position = Vector3.Lerp(itemWorldPosition, forecastWorldPosition, lerpCoefficient);
            forecastItem.transform.position = Vector3.Lerp(forecastWorldPosition, itemWorldPosition, lerpCoefficient);
        }
    }

    /// <summary>
    /// 是否有动画在播放
    /// </summary>
    private void IsMoving()
    {
        if (DOTween.PlayingTweens() == null)
        {
            animationIsReady = true;
        }
        else
        {
            animationIsReady = false;
        }
    }

    public void Swap()
    {
        // 交换 Citems 数组中的 ItouchItem 和 IforecastItem
        int touchX = ItouchItem.x;
        int touchY = ItouchItem.y;
        int forecastX = IforecastItem.x;
        int forecastY = IforecastItem.y;
        GameObject tempGO = items[touchX, touchY];
        // 交换Item对象
        (Citems[touchX, touchY], Citems[forecastX, forecastY]) = (Citems[forecastX, forecastY], Citems[touchX, touchY]);

        // 更新交换后的位置索引
        Citems[touchX, touchY].SetXY(touchX, touchY);
        Citems[forecastX, forecastY].SetXY(forecastX, forecastY);

        // 交换 items 数组中的 touchItem 和 forecastItem
        items[touchX, touchY] = items[forecastX, forecastY];
        items[forecastX, forecastY] = tempGO;
        if (CheatMap()) //成功匹配了应该做什么
        {
            GameManager.Instance.drag = true;
            //UIUpdate.Instance.CountUpdate(--GameManager.Instance.count);
        }
        else //交换失败,交换回去
        {
            // 交换Item对象
            (Citems[forecastX, forecastY], Citems[touchX, touchY]) =
                (Citems[touchX, touchY], Citems[forecastX, forecastY]);

            // 更新交换后的位置索引
            Citems[forecastX, forecastY].SetXY(forecastX, forecastY);
            Citems[touchX, touchY].SetXY(touchX, touchY);

            // 交换 items 数组中的 touchItem 和 forecastItem
            items[forecastX, forecastY] = items[touchX, touchY];
            items[touchX, touchY] = tempGO;
            items[forecastX, forecastY].transform
                .DOMove(new Vector3(initPosition.x + forecastX * offset, initPosition.y + forecastY * offset, 0),
                    0.75f);
            items[touchX, touchY].transform.DOMove(
                new Vector3(initPosition.x + touchX * offset, initPosition.y + touchY * offset, 0),
                0.75f);
        }


        forecastItem = null;
        IforecastItem = null;
    }

    /// <summary>
    /// 物品下落
    /// </summary>
    private void Fall()
    {
        for (int i = 0; i < width; i++)
        {
            Queue<Item> x = new Queue<Item>();
            Queue<GameObject> y = new Queue<GameObject>();
            for (int j = 0; j < height; j++)
            {
                if (Citems[i, j])
                {
                    x.Enqueue(Citems[i, j]);
                    y.Enqueue(items[i, j]);
                }
            }

            for (int j = 0; j < height; j++)
            {
                Citems[i, j] = null;
                items[i, j] = null;
                if (x.Count > 0)
                {
                    Citems[i, j] = x.Dequeue();
                    Citems[i, j].SetXY(i, j);
                    items[i, j] = y.Dequeue();
                    items[i, j].transform
                        .DOMove(new Vector3(initPosition.x + i * offset, initPosition.y + j * offset, 0), j / 10f);
                }
            }

            for (int j = 0; j < height; j++)
            {
                if (!items[i, j])
                {
                    CreateItem(i, j, height + 1);
                }
            }
        }
    }

    private void Removed()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (Citems[i, j].isRemove == true)
                {
                    if (GameManager.Instance.gamePlaying)
                    {
                        GameManager.Instance.score += Initscore;
                    }

                    Destroy(Citems[i, j].gameObject);
                    Citems[i, j] = null;
                }
            }
        }
    }

    /// <summary>
    /// 给即将消除的方块打标签
    /// </summary>
    private void ChangeIsRemove()
    {
        if (waitRemoveItems.Count <= 0) return;
        foreach (var item in waitRemoveItems)
        {
            AddList(item.specialType, item.x, item.y);
        }

        waitRemoveItems.Clear();
        if (specialItems.Count <= 0) return;
        foreach (var item in specialItems)
        {
            item.isRemove = false;
        }

        specialItems.Clear();
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    private bool CreatMap()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                CreateItem(j, i);
            }
        }

        return true;
    }

    /// <summary>
    /// 检测是否有符合消除条件的方块
    /// </summary>
    /// <returns>输出为true时启动消除流程</returns>
    private bool CheatMap()
    {
        //横向检测
        for (int i = 0; i < height; i++)
        {
            int count = 0;
            ItemColor x, y;
            bool non = false;
            x = Citems[0, i].color;
            for (int j = 1; j < width; j++)
            {
                y = Citems[j, i].color;
                if (x == y)
                {
                    count++;
                    non = true;
                }
                else
                {
                    x = y;
                    non = false;
                }

                if (!(j + 1 < width) && non)
                {
                    non = false;
                    j++;
                }

                if (count >= 2 && non == false)
                {
                    if (count == 2)
                        for (int k = count; k >= 0; k--)
                        {
                            waitRemoveItems.Add(Citems[j - k - 1, i]);
                        }
                    else
                    {
                        for (int k = count - 1; k >= 0; k--)
                        {
                            waitRemoveItems.Add(Citems[j - k - 1, i]);
                        }

                        Citems[j - count - 1, i].specialType = ItemType.Vertical;
                        specialItems.Add(Citems[j - count - 1, i]);
                    }

                    count = 0;
                }
                else if (count < 2 && non == false)
                {
                    count = 0;
                }
            }
        }

        //竖直检测
        for (int i = 0; i < width; i++)
        {
            int count = 0;
            ItemColor x, y;
            bool non = false;
            x = Citems[i, 0].color;
            for (int j = 1; j < height; j++)
            {
                y = Citems[i, j].color;
                if (x == y)
                {
                    count++;
                    non = true;
                }
                else
                {
                    x = y;
                    non = false;
                }

                if (!(j + 1 < height) && non)
                {
                    non = false;
                    j++;
                }

                if (count >= 2 && non == false)
                {
                    if (count == 2)
                        for (int k = count; k >= 0; k--)
                        {
                            waitRemoveItems.Add(Citems[i, j - k - 1]);
                        }
                    else
                    {
                        for (int k = count - 1; k >= 0; k--)
                        {
                            waitRemoveItems.Add(Citems[i, j - k - 1]);
                        }

                        Citems[i, j - count - 1].specialType = ItemType.Across;
                        specialItems.Add(Citems[i, j - count - 1]);
                    }

                    count = 0;
                }
                else if (count < 2 && non == false)
                {
                    count = 0;
                }
            }
        }

        if (waitRemoveItems.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CreateItem(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        items[x, y] = Instantiate(itemPrefab, new Vector3(initPosition.x + x * offset, initPosition.y + y * offset, 0),
            quaternion.identity,
            pItem.transform);
        items[x, y].name = $"Item {x} {y} ";
        Citems[x, y] = items[x, y].GetComponent<Item>();
        Citems[x, y].SetXY(x, y);
        Citems[x, y].specialType = ItemType.Normal;
        Citems[x, y].color = (ItemColor)Random.Range(0, (int)ItemColor.Count);

        return true;
    }

    private bool CreateItem(int x, int y, int point)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        items[x, y] = Instantiate(itemPrefab,
            new Vector3(initPosition.x + x * offset, initPosition.y + point * offset, 0),
            quaternion.identity,
            pItem.transform);
        items[x, y].name = $"Item {x} {y} ";
        items[x, y].transform.DOMove(new Vector3(initPosition.x + x * offset, initPosition.y + y * offset, 0), y / 10f);
        Citems[x, y] = items[x, y].GetComponent<Item>();
        Citems[x, y].SetXY(x, y);
        Citems[x, y].specialType = ItemType.Normal;
        Citems[x, y].color = (ItemColor)Random.Range(0, (int)ItemColor.Count);

        return true;
    }


    private bool AddList(ItemType itemType, int x, int y)
    {
        switch (itemType)
        {
            case ItemType.Normal:
                AddItem(x, y);
                break;
            case ItemType.Across:
                AddAcItem(x, y);
                break;
            case ItemType.Vertical:
                AddVItem(x, y);
                break;
            case ItemType.Cross:
                AddCItem(x, y);
                break;
            case ItemType.Area:
                AddAreaItem(x, y, 2);
                break;
            default:
                return false;
        }

        return true;
    }

    private bool AddItem(int x, int y)
    {
        Citems[x, y].isRemove = true;
        return true;
    }

    private bool AddVItem(int x, int y)
    {
        for (int i = 0; i < width; i++)
        {
            AddItem(i, y);
        }

        return true;
    }

    private bool AddAcItem(int x, int y)
    {
        for (int i = 0; i < height; i++)
        {
            AddItem(x, i);
        }

        return true;
    }

    private bool AddCItem(int x, int y)
    {
        AddVItem(x, y);
        AddAcItem(x, y);
        return true;
    }

    private bool AddAreaItem(int x, int y, int size)
    {
        if (x + 1 < width && y - 1 >= 0)
        {
            AddItem(x, y);
            AddItem(x + 1, y);
            AddItem(x, y - 1);
            AddItem(x + 1, y - 1);
        }
        else if (!(x + 1 < width) && y - 1 >= 0)
        {
            AddItem(x, y);
            AddItem(x, y - 1);
        }
        else if (x + 1 < width && !(y - 1 >= 0))
        {
            AddItem(x, y);
            AddItem(x + 1, y);
        }
        else
        {
            AddItem(x, y);
        }

        return true;
    }
}