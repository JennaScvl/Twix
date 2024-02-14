using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerDownHandler, IPointerUpHandler
{
	public bool dragOnSurfaces = true;

	private GameObject m_DraggingIcon;
	private RectTransform m_DraggingPlane;

	public Vector3 mouseOffset;

	public float aspectRatio;
	public float scale;

	public void  OnPointerDown(PointerEventData data)
	{
		if(data.pointerId == -1 && touchScreen)
		{
			processTouch(data);
		}
		if(data.pointerId < -1)
		{
			transform.SetSiblingIndex (1);
		}
	}

	public void Awake()
	{
		RectTransform rt = transform as RectTransform;
		string screen  = (touchScreen?"bottom":"top");

		if(PlayerPrefs.HasKey(string.Format("{0}_PosX",screen)))
		{
			float x = PlayerPrefs.GetFloat(string.Format("{0}_PosX",screen));
			float y = PlayerPrefs.GetFloat(string.Format("{0}_PosY",screen));
			rt.position = new Vector3(x,y);
		}

		if(PlayerPrefs.HasKey(string.Format("{0}_Scale",screen)))
		{
			scale = PlayerPrefs.GetFloat(string.Format("{0}_Scale",screen));
			rt.localScale = new Vector3(scale,scale,1);
		}

		bool isFront = (PlayerPrefs.GetString("FrontScreen") == screen);
		if(isFront)
			transform.SetSiblingIndex(1);
		
	}

	public void  OnPointerUp(PointerEventData data)
	{
		if(data.pointerId == -1 && touchScreen)
		{
			touchPos = new Vector3(touchPos.x, touchPos.y,0);
		}
		if(data.pointerId < -1)
		{
			RectTransform rt = transform as RectTransform;
			string screen = (touchScreen?"bottom":"top");
			PlayerPrefs.SetFloat(string.Format("{0}_PosX",screen), rt.position.x);
			PlayerPrefs.SetFloat(string.Format("{0}_PosY",screen), rt.position.y);
			PlayerPrefs.SetFloat(string.Format("{0}_Scale",screen), scale);
			if(transform.GetSiblingIndex() == 1)
				PlayerPrefs.SetString("FrontScreen", screen);
			PlayerPrefs.Save();
		}
	}

	public void OnBeginDrag(PointerEventData data)
	{
		var canvas = FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;

		m_DraggingIcon = gameObject;
		RectTransform rt = transform as RectTransform;

		if (dragOnSurfaces)
			m_DraggingPlane = transform as RectTransform;
		else
			m_DraggingPlane = canvas.transform as RectTransform;

		Vector3 globalMousePos;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
		{
			mouseOffset = globalMousePos - rt.position;
		}

		SetDraggedPosition(data);
	}

	public void OnScroll(PointerEventData eventData)
	{
		var canvas = FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;

		float scrollDelta = eventData.scrollDelta.y;

		scale += scrollDelta*10;

		scale = Mathf.Max(scale, 32);

		RectTransform rt = transform as RectTransform;
		rt.localScale = new Vector3(scale, scale, 1);
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_DraggingIcon != null)
			SetDraggedPosition(data);
	}

	public bool touchScreen = false;


	public Vector3 touchPos;
	public Rect uiRect;

	private void processTouch(PointerEventData data)
	{
		Vector2 localMousePos;
		Vector2 tp;
		RectTransform rt = transform as RectTransform;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, data.position, data.pressEventCamera, out localMousePos))
		{
			touchPos = new Vector3(Mathf.Clamp01((localMousePos.x*0.75f)+0.5f), 1f-Mathf.Clamp01((localMousePos.y)+.5f),1);
		}
	}

	private void SetDraggedPosition(PointerEventData data)
	{
		if(data.pointerId == -1 && touchScreen)
		{
			processTouch(data);
			return;
		}

		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
			m_DraggingPlane = data.pointerEnter.transform as RectTransform;

		var rt = transform as RectTransform;
		Vector3 globalMousePos;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
		{
			rt.position = globalMousePos - mouseOffset;
			rt.rotation = m_DraggingPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		touchPos = Vector3.zero;
	}

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();

		if (comp != null)
			return comp;

		Transform t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}
}