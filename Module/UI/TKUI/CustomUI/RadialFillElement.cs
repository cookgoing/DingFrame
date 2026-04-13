using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

/*
	图片以中心点，做百分比显示
	https://www.youtube.com/watch?v=yivtEeoHBC8
	https://gist.github.com/Okay-Roman/8ba84316968cd3aac72f3984ad5a6251
*/
namespace DingFrame.Module.TKUI
{
	[UxmlElement]
	public partial class RadialFillElement : VisualElement, INotifyValueChanged<float>
	{
		public enum FillDirectionEN { Clockwise, AntiClockwise }

		private float _value = 1;
		private float _width = 20f, _height = 20f;
		private float _angleOffset = 0f;
		private string _overlayImagePath = string.Empty;
		private float _overlayImageScale = 1f;

		[UxmlAttribute]
		public float value
		{
			get => Mathf.Clamp(_value, 0f, 1f);
			set
			{
				if (EqualityComparer<float>.Default.Equals(_value, value)) return;
				if (panel != null)
				{
					using var evt = ChangeEvent<float>.GetPooled(_value, value);
					evt.target = this;
					SetValueWithoutNotify(value);
					SendEvent(evt);
				}
				else
				{
					SetValueWithoutNotify(value);
				}
			}
		}
		[UxmlAttribute]
		public float width
		{
			get => _width;
			set
			{
				_width = value;
				style.width = value;
			}
		}
		[UxmlAttribute]
		public float height
		{
			get => _height; set
			{
				_height = value;
				style.height = value;
			}
		}
		[UxmlAttribute]
		public string overlayImagePath
		{
			get => _overlayImagePath;
			set
			{
				_overlayImagePath = value;
#if UNITY_EDITOR
				overlayImage.style.backgroundImage = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(value);
#endif
			}
		}
		[UxmlAttribute]
		public float overlayImageScale
		{
			get => _overlayImageScale;
			set
			{
				_overlayImageScale = value;
				overlayImage.style.scale = new Scale(new Vector2(overlayImageScale, overlayImageScale));
			}
		}
		[UxmlAttribute]
		public float angleOffset
		{
			get => _angleOffset;
			set
			{
				_angleOffset = value;
				radialFill.style.rotate = Quaternion.Euler(0, 0, value);
				overlayImage.style.rotate = Quaternion.Euler(0, 0, -value);
			}
		}
		[UxmlAttribute] public Color fillColor { get; set; } = Color.white;
		[UxmlAttribute] public FillDirectionEN fillDirection { get; set; } = FillDirectionEN.Clockwise;

		private float radius => (width > height) ? width / 2 : height / 2;
		private readonly VisualElement radialFill;
		private readonly VisualElement overlayImage;

		public RadialFillElement()
		{
			name = "radial-fill-element";
			Clear();

			radialFill = new VisualElement() { name = "radial-fill" };
			overlayImage = new VisualElement() { name = "overlay-image" };

			Add(radialFill);
			radialFill.Add(overlayImage);

			radialFill.style.flexGrow = 1;
			overlayImage.style.flexGrow = 1;

			radialFill.generateVisualContent += OnGenerateVisualContent;
		}

		public void SetValueWithoutNotify(float newValue)
		{
			_value = Mathf.Clamp(newValue, 0f, 1f);
			radialFill.MarkDirtyRepaint();
		}

		public void OnGenerateVisualContent(MeshGenerationContext mgc)
		{
			int vertextCount = 3;
			int indexCount = 3;
			_value = Mathf.Clamp(_value, 0, 1);
			float degree = _value * 360;
			if (degree <= 240 && degree > 120)
			{
				vertextCount = 4;
				indexCount = 6;
			}
			else if (degree <= 360 && degree > 240)
			{
				vertextCount = 5;
				indexCount = 9;
			}

			MeshWriteData mwd = mgc.Allocate(vertextCount, indexCount);
			Vector3 origin = new ((float)width / 2, (float)height / 2, 0);

			float diameter = 4 * radius;
			float radians = (degree - 90) * Mathf.Deg2Rad;
			float direction = fillDirection == FillDirectionEN.AntiClockwise ? -1 : 1;

			mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(0 * diameter, 0 * diameter, Vertex.nearZ), tint = fillColor });
			mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(0 * diameter, -1 * diameter, Vertex.nearZ), tint = fillColor });

			mwd.SetNextIndex(0);
			mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)2 : (ushort)1);
			if (degree <= 120)
			{
				mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(radians) * diameter * direction, Mathf.Sin(radians) * diameter, Vertex.nearZ), tint = fillColor });
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)1 : (ushort)2);
			}
			else if (degree <= 240)
			{
				mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(30 * Mathf.Deg2Rad) * diameter * direction, Mathf.Sin(30 * Mathf.Deg2Rad) * diameter, Vertex.nearZ), tint = fillColor });
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)1 : (ushort)2);
				mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(radians) * diameter * direction, Mathf.Sin(radians) * diameter, Vertex.nearZ), tint = fillColor });
				mwd.SetNextIndex(0);
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)3 : (ushort)2);
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)2 : (ushort)3);
			}
			else
			{
				mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(30 * Mathf.Deg2Rad) * diameter * direction, Mathf.Sin(30 * Mathf.Deg2Rad) * diameter, Vertex.nearZ), tint = fillColor });
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)1 : (ushort)2);
				mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(150 * Mathf.Deg2Rad) * diameter * direction, Mathf.Sin(150 * Mathf.Deg2Rad) * diameter, Vertex.nearZ), tint = fillColor });
				mwd.SetNextIndex(0);
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)3 : (ushort)2);
				mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)2 : (ushort)3);

				if (degree >= 360)
				{
					mwd.SetNextIndex(0);
					mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)1 : (ushort)3);
					mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)3 : (ushort)1);
				}
				else
				{
					mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(radians) * diameter * direction, Mathf.Sin(radians) * diameter, Vertex.nearZ), tint = fillColor });
					mwd.SetNextIndex(0);
					mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)4 : (ushort)3);
					mwd.SetNextIndex((fillDirection == FillDirectionEN.AntiClockwise) ? (ushort)3 : (ushort)4);
				}
			}
		}
	}
}