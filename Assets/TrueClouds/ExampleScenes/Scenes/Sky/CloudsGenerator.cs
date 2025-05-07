using UnityEngine;

namespace TrueClouds.ExampleScenes.Scenes.Sky
{
    public class CloudsGenerator : MonoBehaviour
    {
        public float Density = 2;
        public GameObject[] Prefabs = new GameObject[0];
        public Vector3 StartPos = Vector3.zero;
        public Vector3 EndPos = new Vector3(100, 0, 100);
        public Texture2D HeightMap;

        void Start()
        {
            var cnt = 0;
            var curPos = StartPos;
            while (curPos.z < EndPos.z)
            {
                curPos.z += Random.Range(Density/2, Density*1.5f);
                curPos.x = StartPos.x;
                while (curPos.x < EndPos.x)
                {
                    curPos.x += Random.Range(Density / 5, Density * 5);
                    var x = (int)(HeightMap.width * curPos.x / (EndPos - StartPos).x);
                    var y = (int)(HeightMap.height * curPos.z / (EndPos - StartPos).x);
                    if (HeightMap.GetPixel(x, y).g < 0.75f)
                    {
                        continue;
                    }
                    var height = HeightMap.GetPixel(x, y).g * 46 - 30;
                    var width = HeightMap.GetPixel(x, y).b * 40;
                    height *= 5;
                    width *= 5;
                    curPos.y = 150;
                    var id = Random.Range(0, Prefabs.Length);
                    cnt++;
                    var placed = Instantiate(Prefabs[id], curPos, Quaternion.identity);
                    placed.transform.localScale = new Vector3(
                        width,
                        10,
                        width
                    );
                    placed.transform.parent = transform;
                }
            }
            Debug.Log(cnt);
        }

        void Update()
        {
            transform.position += Vector3.back * Time.deltaTime * 100;
        }
    }
}
