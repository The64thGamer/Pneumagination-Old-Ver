using UnityEngine;
using System;
using Random = System.Random;
using Unity.VisualScripting;

public class Map_UI : MonoBehaviour
{
    [SerializeField] Data_Manager dataManager;
    [SerializeField] Transform map;
    [SerializeField] Camera mapCam;
    [SerializeField] Sprite[] spriteSheet;

    const int chunkSize = 64;
    const int chunkArrayLength = 4096;

    void Start()
    {
        for (int y = -5; y < 6; y++)
        {
            for (int x = -5; x < 6; x++)
            {
                DisplayChunk(GenerateChunk(x, y, dataManager.GetSeed()),x,y);
            }
        }

    }

    private void LateUpdate()
    {
        mapCam.transform.eulerAngles = new Vector3(90, 0, 0);
        mapCam.transform.position = new Vector3(0, 1, 0);
        map.transform.position = Vector3.zero;
    }

    void DisplayChunk(uint[] chunk, int xChunk, int yChunk)
    {
        Color c = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                uint currentSprite = chunk[x + (chunkSize * y)];
                if (currentSprite > 0)
                {
                    GameObject t = new GameObject();
                    t.layer = 5;
                    t.transform.localEulerAngles = new Vector3(90, 0, 0);
                    t.transform.parent = map;
                    t.transform.localPosition = new Vector3(x + (xChunk* chunkSize), 0, y + (yChunk * chunkSize));
                    SpriteRenderer spr = t.AddComponent<SpriteRenderer>();
                    spr.sprite = spriteSheet[currentSprite - 1];
                    spr.color = c;
                }
            }
        }
    }

    //Chunks are 64x64
    uint[] GenerateChunk(int x, int y, int seed)
    {
        uint[] chunk = new uint[4096];
        uint thisChunkVornoi = FindVornoiOfChunk(x, y, seed);

        for (int i = 0; i < 4; i++)
        {
            int xn = 0;
            int yn = 0;

            switch (i)
            {
                case 0:
                    xn = 1;
                    yn = 0;
                    break;
                case 1:
                    xn = 0;
                    yn = -1;
                    break;
                case 2:
                    xn = -1;
                    yn = 0;
                    break;
                case 3:
                    xn = 0;
                    yn = 1;
                    break;
                default:
                    break;
            }

            if (!(xn == 0 && yn == 0))
            {
                Vector2 vornoiPoint = GetXYFromIndex(FindVornoiOfChunk(x + xn, y + yn, seed)) + new Vector2(xn * chunkSize, yn * chunkSize);
                Vector2 path = GetXYFromIndex(thisChunkVornoi);

                bool reverseApply = false;
                if(vornoiPoint.x < path.x)
                {
                    Vector2 temp = path;
                    path = vornoiPoint;
                    vornoiPoint = temp;
                    reverseApply = true;
                }

                while (true)
                {
                    if (reverseApply)
                    {
                        if (path.x == vornoiPoint.x && path.y == vornoiPoint.y)
                        {
                            break;
                        }
                        else if (path.x < chunkSize && path.y < chunkSize && path.x >= 0 && path.y >= 0)
                        {
                            chunk[GetIndexFromXY(path)] = 1;
                        }
                    }
                    else
                    {
                        if (path.x >= chunkSize || path.y >= chunkSize || path.x < 0 || path.y < 0)
                        {
                            break;
                        }
                        else
                        {
                            chunk[GetIndexFromXY(path)] = 1;
                        }
                    }

                    if (vornoiPoint.x == path.x)
                    {
                        if (vornoiPoint.y > path.y)
                        {
                            path += new Vector2(0, -1);
                        }
                        else if (vornoiPoint.y < path.y)
                        {
                            path += new Vector2(0, 1);
                        }
                    }
                    else
                    {
                        float angle = CalculateSlope(path, vornoiPoint);
                        if (vornoiPoint.x > path.x)
                        {
                            if (angle == 1)
                            {
                                path += new Vector2(1, 1);
                            }
                            else if (angle < 1 && angle > -1)
                            {
                                path += new Vector2(1, 0);
                            }
                            else if (angle > 1)
                            {
                                path += new Vector2(0, 1);
                            }
                            else if (angle < -1)
                            {
                                path += new Vector2(0, -1);
                            }
                            else if (angle == -1)
                            {
                                path += new Vector2(1, -1);
                            }
                        }
                    }
                }

                chunk[GetIndexFromXY(GetXYFromIndex(thisChunkVornoi))] = 2;

            }
        }
        return chunk;
    }

    float CalculateSlope(Vector2 point1, Vector2 point2)
    {
        float rise = point2.y - point1.y;
        float run = point2.x - point1.x;

        if (run != 0)
        {
            float slope = rise / run;
            return slope;
        }
        else
        {
            return float.NaN;
        }
    }

    Vector2 GetXYFromIndex(uint index)
    {
        return new Vector2(index % chunkSize, Mathf.Floor(index / chunkSize));
    }

    uint GetIndexFromXY(Vector2 xy)
    {
        return (uint)(xy.x + (chunkSize * xy.y));
    }

    uint FindVornoiOfChunk(int x, int y, int seed)
    {
        Random rnd = new Random(seed ^ Animator.StringToHash(x.ToString() + y.ToString()));

        return (uint)rnd.Next() % chunkArrayLength;
    }

}
