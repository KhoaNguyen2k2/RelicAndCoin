#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RelicAndCoin.Editor
{
    /// <summary>
    /// Công cụ Editor sinh bộ Sprite Pixel Art 16x16 sắc nét mang phong cách Top-Down RPG (Moonlighter).
    /// Menu: RelicAndCoin > Generate Pixel Art Sprites
    /// </summary>
    public static class PixelArtSpriteGenerator
    {
        private const string ArtPath = "Assets/_Project/Art";

        [MenuItem("RelicAndCoin/Generate Pixel Art Sprites")]
        public static void GenerateAllSprites()
        {
            if (!Directory.Exists(ArtPath))
            {
                Directory.CreateDirectory(ArtPath);
            }

            GenerateHeroSprite();
            GenerateSlimeSprite();
            GenerateChestSprite();
            GenerateShopCounterSprite();
            GenerateCustomerSprite();
            GenerateDungeonFloorSprite();
            GenerateDungeonWallSprite();
            GenerateMenuBackgroundSprite();
            GenerateUIButtonFrameSprite();
            GenerateVillageGroundSprite();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ConfigureImporter("Hero_Pixel.png");
            ConfigureImporter("Slime_Pixel.png");
            ConfigureImporter("TreasureChest_Pixel.png");
            ConfigureImporter("ShopCounter_Pixel.png");
            ConfigureImporter("Customer_Pixel.png");
            ConfigureImporter("DungeonFloor_Pixel.png");
            ConfigureImporter("DungeonWall_Pixel.png");
            ConfigureImporter("MenuBackground_Pixel.png");
            ConfigureImporter("UIButtonFrame_Pixel.png", new Vector4(4, 4, 4, 4));
            ConfigureImporter("VillageGround_Pixel.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[PixelArtSpriteGenerator]</color> Đã tạo thành công 10 bộ sprite Pixel Art sắc nét tại " + ArtPath);
        }

        private static void ConfigureImporter(string fileName, Vector4? border = null)
        {
            string fullPath = $"{ArtPath}/{fileName}";
            var importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                if (border.HasValue)
                {
                    importer.spriteBorder = border.Value;
                }
                importer.SaveAndReimport();
            }
        }

        private static void SaveTexture(Texture2D tex, string fileName)
        {
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(ArtPath, fileName);
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(tex);
        }

        // 1. Hero: Nhà thám hiểm áo choàng xanh dương, tóc nâu, mặt rõ nét
        private static void GenerateHeroSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            FillTransparent(tex);

            Color outline = new Color(0.10f, 0.12f, 0.18f, 1f);
            Color hair = new Color(0.38f, 0.22f, 0.12f, 1f);
            Color skin = new Color(0.96f, 0.78f, 0.62f, 1f);
            Color eye = new Color(0.12f, 0.12f, 0.15f, 1f);
            Color tunic = new Color(0.16f, 0.44f, 0.82f, 1f);
            Color tunicDark = new Color(0.11f, 0.30f, 0.60f, 1f);
            Color belt = new Color(0.60f, 0.40f, 0.15f, 1f);
            Color boots = new Color(0.25f, 0.15f, 0.08f, 1f);

            // Tóc & Đầu (y = 11..15)
            DrawBox(tex, 5, 12, 6, 3, hair);
            DrawBox(tex, 6, 10, 4, 3, skin);
            Set(tex, 6, 11, eye);
            Set(tex, 9, 11, eye);

            // Thân áo choàng (y = 5..9)
            DrawBox(tex, 4, 6, 8, 4, tunic);
            DrawBox(tex, 3, 5, 2, 4, tunicDark); // Áo choàng đổ bóng bên trái
            DrawBox(tex, 11, 5, 2, 4, tunicDark); // Áo choàng đổ bóng bên phải

            // Thắt lưng (y = 5)
            DrawBox(tex, 5, 5, 6, 1, belt);

            // Chân & Giày (y = 1..4)
            DrawBox(tex, 5, 2, 2, 3, boots);
            DrawBox(tex, 9, 2, 2, 3, boots);

            ApplyOutline(tex, outline);
            SaveTexture(tex, "Hero_Pixel.png");
        }

        // 2. Slime: Quái Slime đỏ ruby, mắt trắng/đen, phản quang thạch
        private static void GenerateSlimeSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            FillTransparent(tex);

            Color rubyDark = new Color(0.55f, 0.08f, 0.15f, 1f);
            Color rubyMid = new Color(0.85f, 0.14f, 0.25f, 1f);
            Color rubyLight = new Color(1.00f, 0.35f, 0.45f, 1f);
            Color shine = new Color(1.00f, 0.85f, 0.90f, 1f);
            Color outline = new Color(0.35f, 0.04f, 0.08f, 1f);

            // Thân slime vòm (y = 2..12)
            DrawBox(tex, 3, 2, 10, 4, rubyDark);
            DrawBox(tex, 4, 5, 8, 5, rubyMid);
            DrawBox(tex, 5, 10, 6, 2, rubyLight);

            // Đốm sáng trên đỉnh (highlight bóng thạch)
            Set(tex, 6, 11, shine);
            Set(tex, 7, 11, shine);
            Set(tex, 7, 10, shine);

            // Mắt to ngộ nghĩnh
            Set(tex, 5, 6, Color.white);
            Set(tex, 6, 6, Color.black);
            Set(tex, 9, 6, Color.white);
            Set(tex, 10, 6, Color.black);

            ApplyOutline(tex, outline);
            SaveTexture(tex, "Slime_Pixel.png");
        }

        // 3. TreasureChest: Rương kho báu gỗ sồi nâu viền kim loại vàng
        private static void GenerateChestSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            FillTransparent(tex);

            Color woodDark = new Color(0.36f, 0.18f, 0.08f, 1f);
            Color woodMid = new Color(0.58f, 0.30f, 0.12f, 1f);
            Color gold = new Color(0.96f, 0.78f, 0.20f, 1f);
            Color goldShine = new Color(1f, 0.95f, 0.50f, 1f);
            Color outline = new Color(0.18f, 0.08f, 0.03f, 1f);

            // Thân rương (y = 2..8)
            DrawBox(tex, 2, 2, 12, 7, woodMid);
            DrawBox(tex, 2, 2, 12, 2, woodDark);

            // Nắp rương (y = 8..13)
            DrawBox(tex, 2, 8, 12, 5, woodMid);
            DrawBox(tex, 3, 12, 10, 2, woodDark);

            // Viền vàng kim loại
            DrawBox(tex, 2, 2, 1, 11, gold);
            DrawBox(tex, 13, 2, 1, 11, gold);
            DrawBox(tex, 2, 8, 12, 1, gold);

            // Ổ khóa vàng sáng lấp lánh ở giữa
            DrawBox(tex, 7, 6, 2, 3, gold);
            Set(tex, 7, 8, goldShine);
            Set(tex, 7, 6, outline); // Lỗ khóa

            ApplyOutline(tex, outline);
            SaveTexture(tex, "TreasureChest_Pixel.png");
        }

        // 4. ShopCounter: Quầy bán hàng gỗ phủ khăn trải bàn đỏ viền vàng
        private static void GenerateShopCounterSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            FillTransparent(tex);

            Color wood = new Color(0.48f, 0.28f, 0.14f, 1f);
            Color woodDark = new Color(0.30f, 0.16f, 0.08f, 1f);
            Color clothRed = new Color(0.78f, 0.12f, 0.18f, 1f);
            Color clothGold = new Color(0.94f, 0.76f, 0.22f, 1f);
            Color outline = new Color(0.15f, 0.08f, 0.04f, 1f);

            // Thân quầy gỗ (y = 1..10)
            DrawBox(tex, 1, 1, 14, 9, wood);
            DrawBox(tex, 1, 1, 14, 2, woodDark);

            // Mặt quầy trên (y = 10..13)
            DrawBox(tex, 0, 10, 16, 4, wood);

            // Khăn trải bàn đỏ rủ xuống ở giữa (y = 4..13)
            DrawBox(tex, 4, 5, 8, 8, clothRed);
            // Viền thêu vàng trang nhã của khăn
            DrawBox(tex, 4, 4, 8, 1, clothGold);

            ApplyOutline(tex, outline);
            SaveTexture(tex, "ShopCounter_Pixel.png");
        }

        // 5. Customer: Khách hàng dân làng áo choàng tím/vàng
        private static void GenerateCustomerSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            FillTransparent(tex);

            Color purple = new Color(0.45f, 0.18f, 0.65f, 1f);
            Color purpleDark = new Color(0.28f, 0.08f, 0.42f, 1f);
            Color goldTrim = new Color(0.92f, 0.75f, 0.25f, 1f);
            Color skin = new Color(0.95f, 0.78f, 0.65f, 1f);
            Color outline = new Color(0.12f, 0.05f, 0.18f, 1f);
            Color boots = new Color(0.25f, 0.15f, 0.10f, 1f);

            // Mũ áo choàng trùm đầu (y = 10..15)
            DrawBox(tex, 4, 11, 8, 4, purple);
            DrawBox(tex, 5, 10, 6, 2, skin);
            Set(tex, 6, 11, outline); // Mắt trái
            Set(tex, 9, 11, outline); // Mắt phải

            // Áo choàng thân dài (y = 4..10)
            DrawBox(tex, 3, 4, 10, 7, purple);
            DrawBox(tex, 3, 4, 2, 7, purpleDark);
            DrawBox(tex, 11, 4, 2, 7, purpleDark);
            // Viền vàng áo choàng
            DrawBox(tex, 7, 4, 2, 6, goldTrim);

            // Giày (y = 1..3)
            DrawBox(tex, 5, 1, 2, 3, boots);
            DrawBox(tex, 9, 1, 2, 3, boots);

            ApplyOutline(tex, outline);
            SaveTexture(tex, "Customer_Pixel.png");
        }

        // 6. DungeonFloor: Gạch lát sàn đá xám xanh có vân nứt
        private static void GenerateDungeonFloorSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);

            Color stoneBase = new Color(0.36f, 0.40f, 0.44f, 1f);
            Color stoneLight = new Color(0.44f, 0.48f, 0.52f, 1f);
            Color stoneDark = new Color(0.28f, 0.31f, 0.35f, 1f);
            Color crack = new Color(0.18f, 0.20f, 0.24f, 1f);

            // Phủ nền
            DrawBox(tex, 0, 0, 16, 16, stoneBase);

            // Khối gạch ghép 4 viên nhỏ (mỗi viên 7x7)
            DrawBox(tex, 1, 9, 6, 6, stoneLight);
            DrawBox(tex, 9, 9, 6, 6, stoneBase);
            DrawBox(tex, 1, 1, 6, 6, stoneBase);
            DrawBox(tex, 9, 1, 6, 6, stoneDark);

            // Rãnh chỉ vôi giữa các viên gạch
            DrawBox(tex, 0, 7, 16, 1, stoneDark);
            DrawBox(tex, 7, 0, 1, 16, stoneDark);

            // Đường nứt tự nhiên ở viên gạch góc trên
            Set(tex, 3, 12, crack);
            Set(tex, 4, 13, crack);
            Set(tex, 5, 13, crack);
            Set(tex, 5, 14, crack);

            SaveTexture(tex, "DungeonFloor_Pixel.png");
        }

        // 7. DungeonWall: Tường gạch đá nâu rêu phong có chiều sâu 2.5D
        private static void GenerateDungeonWallSprite()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);

            Color topMoss = new Color(0.22f, 0.34f, 0.26f, 1f); // Rêu trên đỉnh tường
            Color topStone = new Color(0.38f, 0.36f, 0.34f, 1f);
            Color frontStone = new Color(0.25f, 0.20f, 0.18f, 1f); // Mặt tường nhìn nghiêng
            Color frontDark = new Color(0.16f, 0.12f, 0.10f, 1f);
            Color mortar = new Color(0.10f, 0.08f, 0.06f, 1f);

            // Mặt đỉnh tường (y = 11..15)
            DrawBox(tex, 0, 11, 16, 5, topStone);
            DrawBox(tex, 2, 13, 5, 2, topMoss);
            DrawBox(tex, 10, 12, 4, 3, topMoss);

            // Mặt đứng tường gạch đá (y = 0..10)
            DrawBox(tex, 0, 0, 16, 11, frontStone);

            // Mạch vữa giữa các khối đá
            DrawBox(tex, 0, 5, 16, 1, mortar);
            DrawBox(tex, 7, 6, 1, 5, mortar);
            DrawBox(tex, 12, 0, 1, 5, mortar);
            DrawBox(tex, 4, 0, 1, 5, mortar);

            // Đổ bóng chân tường (y = 0..1)
            DrawBox(tex, 0, 0, 16, 2, frontDark);

            SaveTexture(tex, "DungeonWall_Pixel.png");
        }

        // 8. MenuBackground: Bầu trời đêm tím sẫm, trăng sao, núi xa, tiệm cổ kính đèn vàng (160x90)
        private static void GenerateMenuBackgroundSprite()
        {
            const int w = 160;
            const int h = 90;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color skyTop = new Color(0.06f, 0.04f, 0.15f, 1f);     // Tím đen sâu thẳm
            Color skyMid = new Color(0.12f, 0.08f, 0.24f, 1f);     // Tím huyền bí
            Color skyBottom = new Color(0.24f, 0.12f, 0.32f, 1f);  // Ánh hoàng hôn/chân trời tím hồng
            Color mountainFar = new Color(0.14f, 0.10f, 0.22f, 1f);
            Color forestMid = new Color(0.08f, 0.12f, 0.16f, 1f);
            Color groundDark = new Color(0.10f, 0.14f, 0.12f, 1f);
            Color groundPath = new Color(0.22f, 0.24f, 0.22f, 1f);

            // Bầu trời chuyển sắc (Gradient Sky)
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                Color skyColor = t > 0.5f
                    ? Color.Lerp(skyMid, skyTop, (t - 0.5f) * 2f)
                    : Color.Lerp(skyBottom, skyMid, t * 2f);

                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, skyColor);
            }

            // Sao trời lấp lánh (Twinkling Stars)
            int[] starCoords = {
                12, 82, 28, 76, 45, 84, 62, 79, 78, 86, 95, 75, 110, 84, 142, 80,
                154, 85, 20, 68, 38, 62, 54, 71, 72, 65, 88, 70, 102, 63, 148, 67,
                8, 74, 85, 80, 138, 73, 150, 78, 60, 88, 118, 82, 32, 85, 92, 83
            };
            Color starBright = new Color(0.98f, 0.98f, 0.92f, 1f);
            Color starSoft = new Color(0.75f, 0.80f, 0.95f, 0.8f);
            Color starAmber = new Color(1.0f, 0.88f, 0.65f, 0.9f);

            for (int i = 0; i < starCoords.Length; i += 2)
            {
                int sx = starCoords[i];
                int sy = starCoords[i + 1];
                Color sc = (i % 6 == 0) ? starAmber : (i % 4 == 0 ? starSoft : starBright);
                Set(tex, sx, sy, sc);
                if (i % 8 == 0) // Sao lớn có ánh quang chữ thập 4 hướng
                {
                    Set(tex, sx + 1, sy, new Color(sc.r, sc.g, sc.b, 0.4f));
                    Set(tex, sx - 1, sy, new Color(sc.r, sc.g, sc.b, 0.4f));
                    Set(tex, sx, sy + 1, new Color(sc.r, sc.g, sc.b, 0.4f));
                    Set(tex, sx, sy - 1, new Color(sc.r, sc.g, sc.b, 0.4f));
                }
            }

            // Mặt trăng tròn lãng mạn (Moon at x=126, y=68, r=7)
            int mx = 126, my = 68;
            Color moonBody = new Color(0.98f, 0.96f, 0.84f, 1f);
            Color moonCrater = new Color(0.86f, 0.85f, 0.74f, 1f);
            Color moonGlow = new Color(0.98f, 0.96f, 0.84f, 0.25f);

            for (int dy = -10; dy <= 10; dy++)
            {
                for (int dx = -10; dx <= 10; dx++)
                {
                    int distSq = dx * dx + dy * dy;
                    if (distSq <= 49) // R = 7
                    {
                        bool isCrater = (dx == -2 && dy == 2) || (dx == -1 && dy == 3) || (dx == 3 && dy == -1) || (dx == 2 && dy == -2);
                        Set(tex, mx + dx, my + dy, isCrater ? moonCrater : moonBody);
                    }
                    else if (distSq <= 75) // Vầng hào quang dịu nhẹ
                    {
                        Set(tex, mx + dx, my + dy, moonGlow);
                    }
                }
            }

            // Dãy núi tím xa xăm (Distant Mountains)
            for (int x = 0; x < w; x++)
            {
                int mHeight = 28 + (int)(Mathf.Sin(x * 0.06f) * 6f + Mathf.Sin(x * 0.14f) * 3f);
                for (int y = 0; y <= mHeight; y++)
                {
                    Set(tex, x, y, mountainFar);
                }
            }

            // Rừng thông & đồi trung cảnh (Midground Hills)
            for (int x = 0; x < w; x++)
            {
                int fHeight = 18 + (int)(Mathf.Sin(x * 0.08f + 1.2f) * 4f);
                for (int y = 0; y <= fHeight; y++)
                {
                    Set(tex, x, y, forestMid);
                }
            }

            // Mặt đất làng cổ (Village Ground y=0..14)
            DrawBox(tex, 0, 0, w, 14, groundDark);
            // Lối đi lát đá cuội dẫn vào tiệm
            for (int x = 0; x < w; x++)
            {
                int pathY = (x > 30 && x < 80) ? 6 : 4;
                Set(tex, x, pathY, groundPath);
                if (x % 3 == 0) Set(tex, x, pathY - 1, groundPath);
            }

            // Cửa tiệm cổ kính Relic & Coin (Shop/Tavern tại x=22..64, y=10..42)
            Color roofWood = new Color(0.38f, 0.18f, 0.12f, 1f);
            Color roofDark = new Color(0.25f, 0.11f, 0.07f, 1f);
            Color wallWood = new Color(0.28f, 0.16f, 0.10f, 1f);
            Color wallStone = new Color(0.20f, 0.18f, 0.20f, 1f);
            Color timberFrame = new Color(0.14f, 0.08f, 0.05f, 1f);
            Color warmLight = new Color(1.0f, 0.85f, 0.32f, 1f);
            Color warmGlow = new Color(1.0f, 0.70f, 0.20f, 0.45f);
            Color chimneyBrick = new Color(0.32f, 0.15f, 0.15f, 1f);

            // Tường tầng 1 (đá & gỗ)
            DrawBox(tex, 24, 10, 38, 14, wallStone);
            DrawBox(tex, 24, 18, 38, 8, wallWood);
            // Khung xà gỗ cổ điển (Half-timber framing)
            DrawBox(tex, 24, 10, 2, 16, timberFrame);
            DrawBox(tex, 60, 10, 2, 16, timberFrame);
            DrawBox(tex, 42, 10, 2, 16, timberFrame);
            DrawBox(tex, 24, 17, 38, 2, timberFrame);

            // Mái ngói tam giác dốc (Slanted Shingle Roof y=24..40)
            for (int r = 0; r < 14; r++)
            {
                int rx = 20 + r;
                int rw = 46 - (r * 2);
                DrawBox(tex, rx, 24 + r, rw, 1, (r % 2 == 0) ? roofWood : roofDark);
            }

            // Ống khói gạch nung (Chimney)
            DrawBox(tex, 54, 34, 5, 8, chimneyBrick);
            Set(tex, 53, 42, chimneyBrick); // Gờ đỉnh ống khói
            Set(tex, 59, 42, chimneyBrick);
            // Làn khói trắng lam nhẹ lượn lờ
            Set(tex, 56, 43, new Color(0.8f, 0.8f, 0.85f, 0.5f));
            Set(tex, 57, 44, new Color(0.8f, 0.8f, 0.85f, 0.4f));
            Set(tex, 59, 46, new Color(0.8f, 0.8f, 0.85f, 0.3f));
            Set(tex, 61, 48, new Color(0.8f, 0.8f, 0.85f, 0.2f));

            // Cửa chính ấm cúng bằng gỗ sồi
            DrawBox(tex, 40, 10, 6, 7, roofDark);
            Set(tex, 44, 13, warmLight); // Nắm đấm cửa đồng vàng

            // Cửa sổ kính phát ánh đèn vàng ấm rực rỡ (Warm Glowing Windows)
            DrawBox(tex, 28, 12, 8, 5, warmLight);
            DrawBox(tex, 48, 12, 8, 5, warmLight);
            // Chấn song gỗ cửa sổ
            DrawBox(tex, 31, 12, 1, 5, timberFrame);
            DrawBox(tex, 51, 12, 1, 5, timberFrame);
            DrawBox(tex, 28, 14, 8, 1, timberFrame);
            DrawBox(tex, 48, 14, 8, 1, timberFrame);

            // Đèn lồng treo trước cửa tiệm tỏa quầng sáng ấm (Lantern)
            DrawBox(tex, 37, 14, 2, 3, warmLight);
            Set(tex, 37, 17, timberFrame); // Dây xích treo
            // Quầng sáng ấm áp tỏa ra mặt đất phía trước tiệm
            for (int dy = -4; dy <= 4; dy++)
            {
                for (int dx = -6; dx <= 6; dx++)
                {
                    int py = 9 + dy;
                    int px = 38 + dx;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        Color cur = tex.GetPixel(px, py);
                        Color blended = Color.Lerp(cur, warmGlow, 0.35f);
                        tex.SetPixel(px, py, blended);
                    }
                }
            }

            SaveTexture(tex, "MenuBackground_Pixel.png");
        }

        // 9. UIButtonFrame: Khung nút gỗ tối viền vàng đồng cổ điển hỗ trợ 9-slice (32x16)
        private static void GenerateUIButtonFrameSprite()
        {
            const int w = 32;
            const int h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color outerBorder = new Color(0.10f, 0.06f, 0.04f, 1f);  // Viền ngoài đậm 1px
            Color goldHighlight = new Color(0.96f, 0.82f, 0.35f, 1f); // Vàng sáng góc trên/trái
            Color goldMid = new Color(0.82f, 0.65f, 0.22f, 1f);       // Vàng đồng trung tính
            Color goldShadow = new Color(0.48f, 0.32f, 0.10f, 1f);    // Vàng đổ bóng góc dưới/phải
            Color woodBase = new Color(0.24f, 0.14f, 0.09f, 1f);      // Gỗ óc chó tối
            Color woodGrain = new Color(0.28f, 0.17f, 0.11f, 1f);     // Vân gỗ sáng nhẹ
            Color woodDark = new Color(0.18f, 0.10f, 0.06f, 1f);      // Đổ bóng mép trong

            // 1. Phủ toàn bộ thân nút nền gỗ tối
            DrawBox(tex, 0, 0, w, h, woodBase);

            // 2. Viền ngoài bao quanh 1px
            for (int x = 0; x < w; x++)
            {
                Set(tex, x, 0, outerBorder);
                Set(tex, x, h - 1, outerBorder);
            }
            for (int y = 0; y < h; y++)
            {
                Set(tex, 0, y, outerBorder);
                Set(tex, w - 1, y, outerBorder);
            }

            // 3. Khung viền kim loại vàng đồng (Metallic Frame y=1..h-2, x=1..w-2)
            // Cạnh trên & trái sáng
            for (int x = 1; x < w - 1; x++)
            {
                Set(tex, x, h - 2, goldHighlight);
                Set(tex, x, h - 3, goldMid);
            }
            for (int y = 1; y < h - 1; y++)
            {
                Set(tex, 1, y, goldHighlight);
                Set(tex, 2, y, goldMid);
            }

            // Cạnh dưới & phải đổ bóng đồng đậm
            for (int x = 1; x < w - 1; x++)
            {
                Set(tex, x, 1, goldShadow);
                Set(tex, x, 2, goldMid);
            }
            for (int y = 1; y < h - 1; y++)
            {
                Set(tex, w - 2, y, goldShadow);
                Set(tex, w - 3, y, goldMid);
            }

            // Đinh tán đúc góc cổ điển (Corner Rivets)
            Set(tex, 2, h - 3, Color.white);
            Set(tex, w - 3, h - 3, goldHighlight);
            Set(tex, 2, 2, goldHighlight);
            Set(tex, w - 3, 2, goldShadow);

            // 4. Lõi gỗ bên trong (Vùng 9-slice x=4..27, y=4..11)
            DrawBox(tex, 4, 4, w - 8, h - 8, woodBase);
            // Đổ bóng cạnh trên mép lõi gỗ
            for (int x = 4; x < w - 4; x++)
            {
                Set(tex, x, h - 4, woodDark);
                if (x % 4 == 0 || x % 5 == 0)
                {
                    Set(tex, x, 6, woodGrain); // Vân gỗ ngang tự nhiên
                    Set(tex, x, 8, woodGrain);
                }
            }

            SaveTexture(tex, "UIButtonFrame_Pixel.png");
        }

        // 10. VillageGround: Nền cỏ rêu thẫm xen kẽ đá cuội tự nhiên lặp lại liền mạch (32x32)
        private static void GenerateVillageGroundSprite()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color grassBase = new Color(0.20f, 0.32f, 0.18f, 1f);     // Xanh rêu thẫm tự nhiên
            Color grassDark = new Color(0.14f, 0.24f, 0.12f, 1f);     // Vùng rêu ẩm đổ bóng
            Color grassSprout = new Color(0.27f, 0.42f, 0.22f, 1f);   // Đọt cỏ non sáng
            Color stoneMid = new Color(0.42f, 0.44f, 0.42f, 1f);      // Đá cuội xám đất
            Color stoneLight = new Color(0.55f, 0.58f, 0.55f, 1f);    // Phản quang đỉnh đá
            Color stoneDark = new Color(0.25f, 0.27f, 0.25f, 1f);     // Bóng chân đá
            Color flowerPetal = new Color(0.96f, 0.88f, 0.40f, 1f);   // Hoa dại vàng nhỏ
            Color flowerCore = new Color(0.95f, 0.55f, 0.20f, 1f);

            // Phủ nền cỏ xanh rêu
            DrawBox(tex, 0, 0, size, size, grassBase);

            // Hạt dither tự nhiên tạo kết cấu thảm thực vật
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if ((x * 7 + y * 13) % 11 == 0)
                        Set(tex, x, y, grassDark);
                    else if ((x * 17 + y * 5) % 13 == 0)
                        Set(tex, x, y, grassSprout);
                }
            }

            // Cụm đá cuội 1 (Góc trên trái: x=6..10, y=20..23)
            DrawBox(tex, 6, 20, 5, 4, stoneMid);
            Set(tex, 7, 23, stoneLight);
            Set(tex, 8, 23, stoneLight);
            DrawBox(tex, 6, 20, 5, 1, stoneDark);

            // Cụm đá cuội 2 (Góc dưới phải: x=22..25, y=6..8)
            DrawBox(tex, 22, 6, 4, 3, stoneMid);
            Set(tex, 23, 8, stoneLight);
            DrawBox(tex, 22, 6, 4, 1, stoneDark);

            // Vài viên sỏi nhỏ rải rác
            Set(tex, 14, 12, stoneLight);
            Set(tex, 14, 11, stoneDark);

            Set(tex, 28, 24, stoneMid);
            Set(tex, 28, 23, stoneDark);

            Set(tex, 3, 5, stoneLight);
            Set(tex, 3, 4, stoneDark);

            // Khóm hoa dại tí hon 4 cánh (x=17, y=18)
            Set(tex, 17, 18, flowerCore);
            Set(tex, 16, 18, flowerPetal);
            Set(tex, 18, 18, flowerPetal);
            Set(tex, 17, 19, flowerPetal);
            Set(tex, 17, 17, flowerPetal);

            SaveTexture(tex, "VillageGround_Pixel.png");
        }

        // ── Helper functions vẽ pixel ──
        private static void FillTransparent(Texture2D tex)
        {
            Color transparent = new Color(0, 0, 0, 0);
            for (int y = 0; y < tex.height; y++)
                for (int x = 0; x < tex.width; x++)
                    tex.SetPixel(x, y, transparent);
        }

        private static void DrawBox(Texture2D tex, int startX, int startY, int width, int height, Color color)
        {
            for (int y = startY; y < startY + height && y < tex.height; y++)
            {
                if (y < 0) continue;
                for (int x = startX; x < startX + width && x < tex.width; x++)
                {
                    if (x < 0) continue;
                    tex.SetPixel(x, y, color);
                }
            }
        }

        private static void Set(Texture2D tex, int x, int y, Color color)
        {
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                tex.SetPixel(x, y, color);
        }

        private static void ApplyOutline(Texture2D tex, Color outlineColor)
        {
            var copy = tex.GetPixels();
            int w = tex.width;
            int h = tex.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    if (copy[index].a == 0f)
                    {
                        // Kiểm tra 4 hướng lân cận
                        bool nearOpaque =
                            (x > 0 && copy[index - 1].a > 0.5f) ||
                            (x < w - 1 && copy[index + 1].a > 0.5f) ||
                            (y > 0 && copy[index - w].a > 0.5f) ||
                            (y < h - 1 && copy[index + w].a > 0.5f);

                        if (nearOpaque)
                        {
                            tex.SetPixel(x, y, outlineColor);
                        }
                    }
                }
            }
        }
    }
}
#endif
