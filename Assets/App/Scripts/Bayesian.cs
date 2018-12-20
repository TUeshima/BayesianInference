using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bayesian {
    public class Gaussian
    {
        public float m_Mu;
        public float m_Sigma;
    }

    static float _SqrtTwoPi;
    public static float SqrtTwoPi
    {
        get
        {
            if(_SqrtTwoPi == 0f)
            {
                _SqrtTwoPi = Mathf.Sqrt(Mathf.PI * 2);
            }
            return _SqrtTwoPi;
        }
    }
    
    public static float GetYGaussian(float mu, float sigma, float x)
    {
        return 1f / (SqrtTwoPi * sigma) * Mathf.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma));
    }

    public static float GetRandomXGaussinan(float mu, float sigma)
    {
        float x = Random.Range(0f, 1f);
        float y = Random.Range(0f, 1f);
        float r = Mathf.Sqrt(-2 * Mathf.Log(x)) * Mathf.Sin(2 * Mathf.PI * y);
        return mu + r * sigma;
    }

    public static Gaussian Adapt(float mu, float sigma, float x, float speed_elastic, float speed_transition)
    {
        float len = Mathf.Abs(x - mu);
        float a = len / sigma;
        a = Mathf.Log(a);
        const float shrink_thre = 1f;
        if (a < -shrink_thre) a = -shrink_thre;
        if (a > shrink_thre) a = shrink_thre;

        sigma *= 1 + a * speed_elastic;
        mu += (x - mu) * speed_transition;
        return new Gaussian()
        {
            m_Sigma = sigma,
            m_Mu = mu
        };
    }
    
}
