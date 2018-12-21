using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class MainImage : MonoBehaviour {

    [SerializeField]
    RawImage m_RawImage;
    [SerializeField]
    Vector2 m_SizeImage;

    public float m_SpeedElastic = 0.02f;
    public float m_SpeedTransition = 0.01f;

    Mat m_Mat;
    Texture2D m_Texture;
    Bayesian.Gaussian m_ProbabilityFunc;
    Bayesian.Gaussian m_ProbabilityAnother;

    float[] m_Samples;

    void DrawGaussian(Bayesian.Gaussian g, Color color)
    {
        float max_y = 0f;
        for (int x = 0; x < m_SizeImage.x; ++x)
        {
            float y = Bayesian.GetYGaussian(g.m_Mu, g.m_Sigma, x);
            if (y > max_y) max_y = y;
        }

        for(int x = 0; x < m_SizeImage.x; ++x)
        {
            float y = Bayesian.GetYGaussian(g.m_Mu, g.m_Sigma, x);
            y = m_SizeImage.y * y / max_y;
            double[] pixel = new double[4]
            {
                color.r*255, color.g*255, color.b*255, 255
            };
            m_Mat.put((int)y, x, pixel);
        }
    }

    void DrawSamples(float[] samples, Color color)
    {
        for(int i = 0; i < samples.Length; ++i)
        {
            int y = (int)(samples[i] * m_Mat.height());
            Imgproc.line(m_Mat, new Point(i, 0), new Point(i, y), new Scalar(color.r * 255, color.g * 255, color.b * 255, 255));
        }
    }

    public Vector2 GetPosCursor()
    {
        Vector2 pos_cursor = new Vector2(m_SizeImage.x * Input.mousePosition.x / Screen.width, m_SizeImage.y * Input.mousePosition.y / Screen.height);
        return pos_cursor;
    }

    void TakeSamples(Bayesian.Gaussian g)
    {
        const int num = 10000;
        m_Samples = new float[m_Mat.width()];
        for(int i = 0; i < num; ++i)
        {
            int x = (int)Bayesian.GetRandomXGaussinan(g.m_Mu, g.m_Sigma);
            if (x < 0 || x >= m_Mat.width()) continue;
            m_Samples[x] += 1f;

        }

        
        float max = m_Samples.Max();
        for(int i = 0; i < m_Samples.Length; ++i)
        {
            m_Samples[i] /= max;
        }
    }

	void Start () {
        m_Mat = new Mat((int)(m_SizeImage.y), (int)(m_SizeImage.x), CvType.CV_8UC4);
        
        m_ProbabilityFunc = new Bayesian.Gaussian()
        {
            m_Mu = m_SizeImage.x / 2,
            m_Sigma = m_SizeImage.x / 4
        };
        m_Texture = new Texture2D(m_Mat.width(), m_Mat.height(), TextureFormat.RGBA32, false);

        m_ProbabilityAnother = new Bayesian.Gaussian()
        {
            m_Mu = m_SizeImage.x / 2 + Random.Range(-0.3f, 0.3f) * m_SizeImage.x,
            m_Sigma = m_SizeImage.x * 0.15f + Random.Range(-0.05f, 0.05f) * m_SizeImage.x
        };
        TakeSamples(m_ProbabilityAnother);
    }
	
    void _update_cursor()
    {
        DrawGaussian(m_ProbabilityFunc, Color.white);
        int x = (int)GetPosCursor().x;
        m_ProbabilityFunc = Bayesian.Adapt(m_ProbabilityFunc.m_Mu, m_ProbabilityFunc.m_Sigma, x, m_SpeedElastic, m_SpeedTransition);
    }

    void _update_another_gaussian()
    {
        DrawSamples(m_Samples, Color.gray);

        DrawGaussian(m_ProbabilityFunc, Color.white);
        int x = (int)Bayesian.GetRandomXGaussinan(m_ProbabilityAnother.m_Mu, m_ProbabilityAnother.m_Sigma);
        m_ProbabilityFunc = Bayesian.Adapt(m_ProbabilityFunc.m_Mu, m_ProbabilityFunc.m_Sigma, x, m_SpeedElastic, m_SpeedTransition);
        DrawGaussian(m_ProbabilityAnother, Color.red);
    }

	void Update () {
        Imgproc.rectangle(m_Mat, new Point(0, 0), new Point(m_SizeImage.x, m_SizeImage.y), new Scalar(0, 0, 0, 255), -1);

        //_update_cursor();
        _update_another_gaussian();

        Utils.matToTexture2D(m_Mat, m_Texture, false, 0, false, false, false);
        m_RawImage.texture = m_Texture;
	}
}
