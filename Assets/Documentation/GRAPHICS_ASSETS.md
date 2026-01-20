# Graphics Assets - Block Blast

## Tổng quan
Thư mục `Assets/Assets/Graphics` chứa tất cả các sprite assets cho game.

## Danh sách Assets

### 🟦 Block Stone Sprites (10 loại)
Các sprite này được sử dụng cho blocks và cells trên board:

1. **blueStone.png** - Đá màu xanh dương
2. **redStone.png** - Đá màu đỏ
3. **greenStone.png** - Đá màu xanh lá
4. **yellowStone.png** - Đá màu vàng
5. **orangeStone.png** - Đá màu cam
6. **purpleStone.png** - Đá màu tím
7. **pinkStone.png** - Đá màu hồng
8. **brownStone.png** - Đá màu nâu
9. **creamStone.png** - Đá màu kem
10. **lightBlueStone.png** - Đá màu xanh nhạt

**Sử dụng:**
- Assign vào `GameConfig.blockStoneSprites[]`
- Random chọn khi spawn block mới
- Hiển thị trên Cell khi block được đặt

---

### 🎮 Gameplay UI Sprites

#### gameplay_board_ground_1.png
- **Mục đích:** Background cho toàn bộ board 8x8
- **Sử dụng:** Assign vào `GameConfig.boardBackgroundSprite`
- **Vị trí:** Dưới các cells

#### gameplay_cell_mid.png / gameplay_cell_mid (1).png
- **Mục đích:** Background cho mỗi cell trên board
- **Sử dụng:** Assign vào `GameConfig.cellBackgroundSprite` và `emptyCellSprite`
- **Vị trí:** Background của mỗi Cell object

---

### 🏆 UI Elements

#### Icon_Trophy.png
- **Mục đích:** Icon cho Best Score
- **Sử dụng:** Hiển thị bên cạnh Best Score text trong UI
- **Vị trí:** Score Panel

#### button.png
- **Mục đích:** Background cho các buttons
- **Sử dụng:** UI Button sprite
- **Vị trí:** Restart button, New Game button, etc.

#### setting_icon.png
- **Mục đích:** Icon settings
- **Sử dụng:** Settings button (nếu có)
- **Vị trí:** Top corner của UI

---

### ✨ Effects

#### combo.png
- **Mục đích:** Icon/Background cho combo display
- **Sử dụng:** ComboEffect, ComboPanel
- **Vị trí:** Giữa màn hình khi có combo

#### combo_bitmap.png
- **Mục đích:** Bitmap font cho combo text
- **Sử dụng:** TextMeshPro hoặc custom text rendering
- **Vị trí:** Combo display

#### x_combo.png
- **Mục đích:** Ký tự "x" cho combo (e.g., "Combo x3")
- **Sử dụng:** Kết hợp với combo number
- **Vị trí:** Combo text

---

### 🌍 Other

#### ground.png
- **Mục đích:** Background/Ground texture
- **Sử dụng:** Scene background hoặc bottom area
- **Vị trí:** Behind everything

---

## Cách Setup trong Unity

### 1. Import Settings
Đảm bảo tất cả sprites có settings đúng:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100 (hoặc tùy theo kích thước)
Filter Mode: Bilinear
Compression: None hoặc High Quality
```

### 2. Auto-Assign vào GameConfig
1. Mở GameConfig ScriptableObject
2. Click "Auto-Assign Stone Sprites from Graphics Folder"
3. Click "Auto-Assign UI Sprites from Graphics Folder"

### 3. Manual Assign
Nếu auto-assign không hoạt động:
1. Kéo từng stone sprite vào `Block Sprites` array
2. Assign UI sprites vào các field tương ứng

---

## Sử dụng trong Code

### Random Block Sprite
```csharp
Sprite randomStone = config.blockStoneSprites[Random.Range(0, config.blockStoneSprites.Length)];
block.Initialize(shape, randomStone, cellSize);
```

### Set Cell Sprite
```csharp
cell.SetFilled(true, stoneSprite, config.cellBackgroundSprite);
```

### Clear Cell
```csharp
cell.SetFilled(false, null, config.cellBackgroundSprite);
```

---

## Tips & Best Practices

### Performance
- Tất cả stone sprites nên có cùng kích thước
- Sử dụng Sprite Atlas để giảm draw calls
- Enable mipmaps nếu có scaling

### Visual Consistency
- Đảm bảo tất cả stones có style nhất quán
- Padding/margin giống nhau
- Lighting/shadow direction giống nhau

### Extensibility
Để thêm stone mới:
1. Thêm sprite vào Graphics folder
2. Đặt tên theo format: `[color]Stone.png`
3. Re-run "Auto-Assign Stone Sprites"
4. Hoặc manually thêm vào array

---

## Thứ tự Render (Sorting Layers)

Recommended sorting order:
```
Background (Order: -10)
└── Board Background (boardBackgroundSprite)

Default (Order: 0)
├── Cell Background (cellBackgroundSprite) - Order: 0
└── Stone Sprite (blockStoneSprites) - Order: 1

UI (Order: 10+)
├── Score Display
├── Combo Effect
└── Buttons
```

---

**Created:** January 19, 2026  
**Last Updated:** January 19, 2026
