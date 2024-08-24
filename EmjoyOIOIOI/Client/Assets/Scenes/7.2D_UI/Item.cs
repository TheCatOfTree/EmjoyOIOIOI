using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public enum ItemType
{
    Normal, // 正常方块
    Across, //横
    Vertical, //竖
    Cross, //交叉
    Area //面积
}

public enum ItemColor
{
    Red = 0,
    Blue,
    Green,
    Yellow,
    Cyan,
    Count
}

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] public int x, y;
    public ItemType specialType; // 方块类型
    public ItemColor color;
    public bool isRemove;
    public ItemTable able;
    private GameObject Map;
    private Map map;
    private Image image;

    public Vector2 lastPosition = Vector2.zero;

    public Item()
    {
    }

    public Item(ItemType a)
    {
        specialType = a;
    }


    void Awake()
    {
        image = GetComponent<Image>();
        Map = able.map;
        map = Map.GetComponent<Map>();
        isRemove = false;
    }

    private void Start()
    {
        //方块类型->image
        switch (specialType)
        {
            case ItemType.Normal:

                break;


            default:
                break;
        }

        //方块颜色->image
        switch (color)
        {
            case ItemColor.Blue:
                image.color = Color.blue;
                break;
            case ItemColor.Green:
                image.color = Color.green;
                break;
            case ItemColor.Red:
                image.color = Color.red;
                break;
            case ItemColor.Yellow:
                image.color = Color.yellow;
                break;
            case ItemColor.Cyan:
                image.color = Color.cyan;
                break;
            default:
                break;
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!map.animationIsReady)
        {
            return; // 有Tween在播放时不允许拖动
        }

        map.touchItem = transform.gameObject;
        map.ItouchItem = map.touchItem.GetComponent<Item>();
        map.initItemPoint = transform.position;
        lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!map.animationIsReady)
        {
            return; // 有Tween在播放时不允许拖动
        }

        // transform.position = eventData.position;
        transform.SetAsLastSibling();
        map.mouseVelocity += (eventData.position - lastPosition);
        map.touchPosition = eventData.position;
        lastPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!map.animationIsReady)
        {
            return; // 有Tween在播放时不允许拖动
        }

        if (map.touchItem&&map.forecastItem)
            map.Swap();
        map.mouseVelocity = Vector2.zero;
    }

    public bool SetXY(int x, int y)
    {
        this.x = x;
        this.y = y;
        return true;
    }
}