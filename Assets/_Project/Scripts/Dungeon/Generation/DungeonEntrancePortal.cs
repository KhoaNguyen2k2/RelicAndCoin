using UnityEngine;

namespace RelicAndCoin.Dungeon.Generation
{
    /// <summary>
    /// Cổng Hầm Ngục: đối tượng Pixel Art 24x24 (cổng vòm đá rêu có lõi xoáy tím huyền bí)
    /// đặt ở góc trên bên phải của lưới hầm ngục, lấy thế điểm vào đầu vào hầm.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DungeonEntrancePortal : MonoBehaviour
    {
        [Header("Vị trí cổng (thế giới)")]
        [SerializeField] private Vector2 _position = new Vector2(5.5f, 2.8f);

        [Header("Kích thước")]
        [SerializeField] private float _size = 1f;
        [SerializeField] private int _pixelsPerUnit = 24;

        private static readonly string PortalAssetPath = "Assets/_Project/Art/DungeonPortal_Pixel.png";

        private void Awake()
        {
            transform.position = _position;
            transform.localScale = Vector3.one * _size;

            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = LoadOrCreatePortalSprite(_pixelsPerUnit);
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 1;
        }

        private static Sprite LoadOrCreatePortalSprite(int pixelsPerUnit)
        {
#if UNITY_EDITOR
            var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(PortalAssetPath);
            if (sprite != null)
                return sprite;
#endif
            return CreateProceduralPortalSprite(pixelsPerUnit);
        }

        private static Sprite CreateProceduralPortalSprite(int pixelsPerUnit)
        {
            const int size = 24;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            // Màu nền trong suốt
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));

            // Vòm đá rêu: khung xám xanh lá tối + viền đen nhạt
            Color stone = new Color(0.5f, 0.55f, 0.5f, 1f);
            Color moss = new Color(0.35f, 0.5f, 0.35f, 1f);
            Color dark = new Color(0.15f, 0.18f, 0.15f, 1f);
            Color purpleCore = new Color(0.8f, 0.3f, 1f, 1f);
            Color purpleGlow = new Color(0.5f, 0.1f, 0.8f, 1f);

            int cx = size / 2;   // 12
            int cy = size / 2;   // 12
            int archTop = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Vòm đá: bán kính ngoài 10, trong 5, từ đỉnh vòm (archTop) xuống dưới
                    bool inOuter = dist <= 10f && y >= archTop - 2;
                    bool inHole = dist <= 5f;

                    if (inOuter && !inHole)
                    {
                        // Viền đá dày 1 ô
                        bool edge = dist > 8.5f;
                        texture.SetPixel(x, y, edge ? dark : (moss + stone) * 0.5f);
                    }

                    // Lõi xoáy tím: bán kính 4.5, bị che bởi đá ở mặt trên
                    if (dist <= 4.5f && y > archTop)
                    {
                        float swirl = Mathf.Sin(dist * 1.2f + Time.time * 0f) * 0.5f + 0.5f;
                        texture.SetPixel(x, y, Color.Lerp(purpleGlow, purpleCore, swirl));
                    }
                }
            }

            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.name = "DungeonEntrancePortal_Sprite";
            return sprite;
        }
    }
}
