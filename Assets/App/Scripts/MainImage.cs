using System.Collections;
using System.Collections.Generic;
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
    unsafe void DrawGaussian(Bayesian.Gaussian g)
    {
        float max_y = 0f;
        for (int x = 0; x < m_SizeImage.x; ++x)
        {
            float y = Bayesian.GetYGaussian(m_ProbabilityFunc.m_Mu, m_ProbabilityFunc.m_Sigma, x);
            if (y > max_y) max_y = y;
        }

        Imgproc.rectangle(m_Mat, new Point(0, 0), new Point(m_SizeImage.x, m_SizeImage.y), new Scalar(0, 0, 0, 255), -1);
        for(int x = 0; x < m_SizeImage.x; ++x)
        {
            float y = Bayesian.GetYGaussian(m_ProbabilityFunc.m_Mu, m_ProbabilityFunc.m_Sigma, x);
            y = m_SizeImage.y * y / max_y;
            double[] pixel = new double[4]
            {
                255, 255, 255, 255
            };
            m_Mat.put((int)y, x, pixel);
        }
    }

    public Vector2 GetPosCursor()
    {
        Vector2 pos_cursor = new Vector2(m_SizeImage.x * Input.mousePosition.x / Screen.width, m_SizeImage.y * Input.mousePosition.y / Screen.height);
        return pos_cursor;
    }

	void Start () {
        m_Mat = new Mat((int)(m_SizeImage.y), (int)(m_SizeImage.x), CvType.CV_8UC4);
        
        m_ProbabilityFunc = new Bayesian.Gaussian()
        {
            m_Mu = m_SizeImage.x / 2,
            m_Sigma = m_SizeImage.x / 4
        };
        m_Texture = new Texture2D(m_Mat.width(), m_Mat.height(), TextureFormat.RGBA32, false);
    }
	
	void Update () {
        DrawGaussian(m_ProbabilityFunc);
        int x = (int)GetPosCursor().x;
        m_ProbabilityFunc = Bayesian.Adapt(m_ProbabilityFunc.m_Mu, m_ProbabilityFunc.m_Sigma, x, m_SpeedElastic, m_SpeedTransition);

        Utils.matToTexture2D(m_Mat, m_Texture, false, 0, false, false, false);
        m_RawImage.texture = m_Texture;
	}
}
