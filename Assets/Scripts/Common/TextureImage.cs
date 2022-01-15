using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WizardSystem.Common
{
    public class TextureImage
    {
        public Texture2D Texture;
        public Color BackgroundColor;

        private int m_size;
        private int m_margin;
        public int Size 
        { 
            get => m_size; 
            set
            {
                m_size = value;
                OnSizeChanged();
            }
        }
        public int Margin 
        { 
            get => m_margin; 
            set
            {
                m_margin = value;
                OnSizeChanged();
            }
        }
        private float m_realSize;
        private float m_halfRealSize;

        public TextureImage()
        {
            m_size = 300;
            m_margin = 10;
            BackgroundColor = Color.white;

            OnSizeChanged();
        }

        public byte[] GetRawImage()
        {
            if (Texture != null)
            {
                return Texture.EncodeToPNG();
            }
            return null;
        }

        public void DrawLines(List<Vector3> points)
        {
            Texture = new Texture2D(Size, Size);
            Texture.SetPixels(Texture.GetPixels().Select(color => color = BackgroundColor).ToArray());

            Vector2 maxPoint = new Vector2(points.Max(x => x.x), points.Max(x => x.y));
            Vector2 minPoint = new Vector2(points.Min(x => x.x), points.Min(x => x.y));
            float originalWidth = maxPoint.x - minPoint.x;
            float originalHeight = maxPoint.y - minPoint.y;
            float originalSize = Mathf.Max(originalWidth, originalHeight);
            float topMargin = Margin + (m_halfRealSize - m_halfRealSize / originalSize * originalHeight);
            float leftMargin = Margin + (m_halfRealSize - m_halfRealSize / originalSize * originalWidth);

            var normalizedPoints = points.Select(point => 
                new Vector2((m_realSize * (point.x - minPoint.x) / originalSize) + leftMargin,
                            (m_realSize * (point.y - minPoint.y) / originalSize) + topMargin)).ToList();

            for (int i = 0; i < normalizedPoints.Count - 1; i++)
            {
                DrawLineOnTexture(Texture, normalizedPoints[i], normalizedPoints[i + 1], Color.black, 3);
            }

            Texture.Apply();
        }

        public void DrawLineOnTexture(Texture2D texture, Vector2 p1, Vector2 p2, Color color, int radius)
        {
            Vector2 drawPoint = p1;
            float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
            float ctr = 0;

            while ((int)drawPoint.x != (int)p2.x || (int)drawPoint.y != (int)p2.y)
            {
                drawPoint = Vector2.Lerp(p1, p2, ctr);
                ctr += frac;
                
                // radius만큼 픽셀 그리기
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (x * x + y * y <= radius * radius)
                        {
                            texture.SetPixel((int)drawPoint.x + x, (int)drawPoint.y + y, color);
                        }
                    }
                }
            }
        }
        
        public void OnSizeChanged()
        {
            m_realSize = Size - Margin * 2;
            m_halfRealSize = m_realSize / 2;
        }
    }
}
