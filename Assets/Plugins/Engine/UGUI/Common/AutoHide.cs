using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float m_LifeTime = 3.0f;

    float m_fElapseTime = 0.0f;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            m_fElapseTime += Time.deltaTime;
            if (m_fElapseTime >= m_LifeTime)
            {
                m_fElapseTime = 0;
                gameObject.SetActive(false);
            }
        }
    }
}
