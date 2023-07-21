using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

[Serializable]
public class ColorEvent : UnityEvent<Color> //This class is used to have unity events in the inspector.
{

}


public class ColorPickerController : MonoBehaviour
{

    [SerializeField] private ColorEvent m_OnColorPreview, m_OnColorSelect;

    public static Color m_ColorChosen; //Static so it can be used in the game scene.

    private RectTransform m_Rect;

    private Texture2D m_ColorTexture;

    private void Start()
    {
        m_Rect = GetComponent<RectTransform>();
        m_ColorTexture = GetComponent<Image>().mainTexture as Texture2D;
        m_ColorChosen = Color.black;
        m_OnColorSelect?.Invoke(Color.black);
    }


    private void Update()
    {
        ColorPickerFunctionality();
    }

    private void ColorPickerFunctionality()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(m_Rect, Input.mousePosition)) //If mouse is inside the color picker rectangle
        {
            Vector2 delta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Rect, Input.mousePosition, null, out delta); //Take the mouse position and convert it to local rect coordinates

            float width = m_Rect.rect.width; //width of the color picker rect.
            float height = m_Rect.rect.height;//height of the color picker rect.

            delta += new Vector2(width * 0.5f, height * 0.5f); //add half of the width and the height to the rectangle. Necessary to accomodate coordinates.

            float x = Mathf.Clamp(delta.x / width, 0, 1); //Clamp them so they never exceed 0 or 1.
            float y = Mathf.Clamp(delta.y / height, 0, 1);

            int TexX = Mathf.RoundToInt(x * m_ColorTexture.width);
            int TexY = Mathf.RoundToInt(y * m_ColorTexture.height);

            Color color = m_ColorTexture.GetPixel(TexX, TexY); //Get the color from the pixel that the mouse is on.

            m_OnColorPreview?.Invoke(color); //Set preview image color to the color the mouse is on.

            if (Input.GetMouseButtonDown(0))
            {
                m_OnColorSelect?.Invoke(color);
                m_ColorChosen = color; //Set color chosen to the one user clicked on
            }
        }
    }

    public Color GetColor()
    {
        return m_ColorChosen;
    }
}
