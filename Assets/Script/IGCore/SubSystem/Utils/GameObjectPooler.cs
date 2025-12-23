using UnityEngine;
using UnityEngine.Pool;

namespace Core.Utils
{
    public interface IGameObjectPooler
    {
        void Create(GameObject src, Transform shelterRoot);
        GameObject Get();
        void Release(GameObject returnObject);
    }

    public class GameObjectPooler : IGameObjectPooler
    {
        IObjectPool<GameObject> Pooler;

        GameObject SourceObject;
        Transform TransformShelter;

        public int DefaultCapacity { get; set; } = 10;
        public int MaxPoolSize { get; set; } = 15;



        // static.
        public static GameObject GetPoolItem(GameObjectPooler pooler, Transform parent, Vector3 vPos = default(Vector3))
        {
            if (pooler == null) return null;

            var itemObj = pooler.Get();
            itemObj.transform.position = vPos;
            itemObj.transform.SetParent(parent, true);
            itemObj.SetActive(true);
            return itemObj;
        }
        public static void ReleasePoolItem(GameObjectPooler pooler, GameObject targetObj)
        {
            if(pooler != null)
                pooler.Release(targetObj);
        }


        // interfaces.
        public void Create(GameObject source, Transform trShelter)
        {
            SourceObject = source;
            TransformShelter = trShelter;
            Pooler = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, DefaultCapacity, MaxPoolSize);
        }
        public GameObject Get()
        {
            return Pooler.Get();
        }
        public void Release(GameObject returnObject)
        {
            Pooler.Release(returnObject);
        }






        GameObject CreatePooledItem()
        {
            GameObject newSymbol = GameObject.Instantiate(SourceObject, TransformShelter);
            newSymbol.SetActive(true);
            newSymbol.name = SourceObject.name;
            return newSymbol;
        }
        void OnTakeFromPool(GameObject gameObj)
        {
            gameObj.SetActive(true);
        }
        void OnReturnedToPool(GameObject gameObj)
        {
            gameObj.transform.SetParent(TransformShelter);
            gameObj.SetActive(false);
        }
        void OnDestroyPoolObject(GameObject gameObj)
        {
            GameObject.Destroy(gameObj);
        }
    }
}