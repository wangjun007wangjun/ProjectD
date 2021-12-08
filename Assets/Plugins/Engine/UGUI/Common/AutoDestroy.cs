using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float m_LifeTime = 3.0f;

    float m_fElapseTime = 0.0f;

    void Update()
    {
        m_fElapseTime += Time.deltaTime;
        if (m_fElapseTime >= m_LifeTime)
            GameObject.DestroyImmediate(gameObject);
    }
}
