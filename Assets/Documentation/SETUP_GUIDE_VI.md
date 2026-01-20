# Hướng dẫn Setup Block Blast Game

## Tổng quan
Dự án đã được tạo base code hoàn chỉnh theo GDD. Bây giờ cần setup trong Unity Editor.

## 📋 Checklist Setup

### 1. Cấu trúc thư mục đã tạo
```
Assets/
├── Scripts/
│   ├── Data/           ✅ BlockShape, BlockShapeData, GameConfig, GameData
│   ├── Core/           ✅ GameManager, BoardManager, Cell, Block, BlockSpawner, etc.
│   ├── UI/             ✅ UIManager, ComboEffect
│   ├── Effects/        ✅ CellDestroyEffect
│   └── Utils/          ✅ GameUtils, AudioManager
```

### 2. Tạo GameConfig (ScriptableObject)
**Bước thực hiện:**
1. Mở Unity Editor
2. Click phải trong Project window
3. Chọn: Create → Block Blast → Game Config
4. Đặt tên: "GameConfig"
5. Trong Inspector, cấu hình:
   ```
   Board Width: 8
   Board Height: 8
   Block Spawn Count: 3
   Cell Size: 1.0
   Block Spacing: 2.0
   Base Score Per Cell: 1
   Base Score Per Line: 8
   Combo Multiplier: 0.1
   Move Count For Combo: 3
   Block Placement Duration: 0.2
   Block Return Duration: 0.3
   Line Clear Delay: 0.3
   Cell Destroy Duration: 0.2
   ```

### 2.1. Assign Sprites vào GameConfig
**Có 2 cách:**

**Cách 1: Tự động (Khuyến nghị)**
1. Select GameConfig trong Project
2. Trong Inspector, kéo xuống cuối
3. Click button "Auto-Assign Stone Sprites from Graphics Folder"
4. Click button "Auto-Assign UI Sprites from Graphics Folder"

**Cách 2: Thủ công**
1. Mở thư mục `Assets/Assets/Graphics`
2. Assign các sprites vào GameConfig:
   ```
   Visual Settings:
   - Empty Cell Sprite: gameplay_cell_mid.png
   - Cell Background Sprite: gameplay_cell_mid.png
   - Board Background Sprite: gameplay_board_ground_1.png
   
   Block Sprites (kéo tất cả stone sprites vào array):
   - blueStone.png
   - redStone.png
   - greenStone.png
   - yellowStone.png
   - orangeStone.png
   - purpleStone.png
   - pinkStone.png
   - brownStone.png
   - creamStone.png
   - lightBlueStone.png
   ```

### 3. Tạo Cell Prefab
**Mục đích:** Đại diện cho 1 ô trên bảng 8x8

**Bước thực hiện:**
1. Tạo GameObject mới: Hierarchy → Right Click → Create Empty
2. Đặt tên: "Cell"
3. Add Component → Sprite Renderer (cho background)
   - Sprite: gameplay_cell_mid.png (từ Graphics folder)
   - Color: White
   - Sorting Layer: Default, Order: 0
4. Add Component → Cell (script)
5. Trong Cell component:
   - Background Renderer: Kéo SpriteRenderer vào (tự động)
   - Stone Renderer: Sẽ tự động tạo khi chạy
6. Kéo vào Project để tạo prefab
7. Xóa Cell trong Hierarchy

### 4. Tạo Block Prefab
**Mục đích:** Đại diện cho block có thể kéo thả

**Bước thực hiện:**
1. Tạo GameObject: "Block"
2. Add Component → Box Collider 2D
   - Size: (1, 1)
   - Is Trigger: True
3. Add Component → Block (script)
   - Cell Prefab: Tạo một prefab nhỏ cho cell trong block:
     - Tạo GameObject mới "BlockCell"
     - Add SpriteRenderer
     - Sprite: Để trống (sẽ set runtime)
     - Lưu thành prefab
     - Assign vào Block component
4. Kéo Block vào Project để tạo prefab
5. Xóa Block trong Hierarchy

### 5. Setup Scene - Board

**Tạo Board Container:**
1. Tạo Empty GameObject: "Board"
2. Position: (0, 2, 0) - Ở giữa-trên màn hình
3. Add Component → Board Manager
   - Cell Prefab: Kéo Cell prefab vào
   - Board Container: Kéo chính Board GameObject vào (self-reference)

### 6. Setup Scene - Block Spawner

**Tạo Block Spawner:**
1. Tạo Empty GameObject: "BlockSpawner"
2. Position: (0, 0, 0)
3. Tạo 3 Empty GameObject con:
   - "SpawnPos1" → Position: (-3, -4, 0)
   - "SpawnPos2" → Position: (0, -4, 0)
   - "SpawnPos3" → Position: (3, -4, 0)
4. Add Component → Block Spawner (vào BlockSpawner cha)
   - Block Prefab: Kéo Block prefab vào
   - Spawn Positions: Size = 3, kéo 3 spawn positions vào

### 7. Setup Scene - Game Manager

**Tạo Game Manager:**
1. Tạo Empty GameObject: "GameManager"
2. Add Component → Game Manager
3. Add Component → Score Manager
4. Add Component → Save Manager
5. Add Component → Block Drag Handler

**Assign References cho Game Manager:**
- Config: Kéo GameConfig ScriptableObject vào
- Board Manager: Kéo Board GameObject vào
- Block Spawner: Kéo BlockSpawner GameObject vào
- Score Manager: Sẽ tự lấy từ cùng GameObject
- Save Manager: Sẽ tự lấy từ cùng GameObject
- UI Manager: (Sẽ tạo ở bước sau)
- Drag Handler: Sẽ tự lấy từ cùng GameObject

**Assign References cho Block Drag Handler:**
- Board Manager: Kéo Board GameObject vào
- Config: Kéo GameConfig ScriptableObject vào

### 8. Setup UI

**Tạo Canvas:**
1. Hierarchy → UI → Canvas
2. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1080 x 1920
   - Match: 0.5

**Tạo Score Panel:**
1. Tạo Panel: Canvas → Right Click → UI → Panel
2. Đặt tên: "ScorePanel"
3. Position: Top của màn hình
4. Tạo 2 Text (TMP):
   - "ScoreText" → Text: "Score: 0"
   - "BestScoreText" → Text: "Best: 0"

**Tạo Combo Panel:**
1. Tạo Panel: "ComboPanel"
2. Position: Giữa màn hình
3. Active: False (ẩn mặc định)
4. Tạo Text (TMP): "ComboText" → Text: "Combo x1"
5. Add Component → Combo Effect (vào ComboPanel)
   - Combo Text: Kéo ComboText vào

**Tạo Game Over Panel:**
1. Tạo Panel: "GameOverPanel"
2. Active: False
3. Tạo Text (TMP): "GameOverTitle" → Text: "GAME OVER"
4. Tạo Text (TMP): "FinalScoreText" → Text: "Final Score: 0"
5. Tạo Button: "RestartButton" → Text: "Restart"

**Tạo Menu Panel:**
1. Tạo Panel: "MenuPanel"
2. Active: False
3. Tạo Button: "NewGameButton" → Text: "New Game"
4. Tạo Button: "LoadGameButton" → Text: "Continue"

**Add UI Manager vào Canvas:**
1. Select Canvas
2. Add Component → UI Manager
3. Assign tất cả references:
   - Score Text: Kéo ScoreText vào
   - Best Score Text: Kéo BestScoreText vào
   - Combo Text: Kéo ComboText vào
   - Combo Panel: Kéo ComboPanel vào
   - Game Over Panel: Kéo GameOverPanel vào
   - Final Score Text: Kéo FinalScoreText vào
   - Restart Button: Kéo RestartButton vào
   - Menu Panel: Kéo MenuPanel vào
   - New Game Button: Kéo NewGameButton vào
   - Load Game Button: Kéo LoadGameButton vào

**Quay lại Game Manager:**
- UI Manager: Kéo Canvas vào (vì UIManager gắn trên Canvas)

### 9. Setup Camera

1. Select Main Camera
2. Position: (0, 0, -10)
3. Projection: Orthographic
4. Size: 7 (điều chỉnh để vừa với board)
5. Background: Màu tối (R:0.1, G:0.1, B:0.1)
6. Clear Flags: Solid Color

### 10. Tạo Sprites (nếu chưa có)

**Cách tạo sprite vuông đơn giản:**
1. Tạo file ảnh 64x64 pixels màu trắng
2. Import vào Unity
3. Texture Type: Sprite (2D and UI)
4. Hoặc dùng built-in: "UI/Skin/Knob"

### 11. Test chạy game

**Kiểm tra:**
1. Bấm Play
2. Kiểm tra:
   - ✅ Bảng 8x8 hiển thị
   - ✅ 3 block xuất hiện ở dưới
   - ✅ Có thể kéo block
   - ✅ Đặt block lên bảng
   - ✅ Điểm tăng
   - ✅ Xóa hàng/cột khi đầy
   - ✅ Sinh block mới sau khi đặt hết 3 block

## 🐛 Troubleshooting

### Block không kéo được
- Kiểm tra Block có BoxCollider2D
- Kiểm tra Camera tagged là "MainCamera"

### UI không hiển thị
- Kiểm tra Canvas Render Mode
- Kiểm tra EventSystem có trong scene

### Không xóa hàng/cột
- Kiểm tra BoardManager.CheckAndClearLines() được gọi
- Kiểm tra logic trong GameManager.OnBlockPlaced()

### Không lưu game
- Kiểm tra SaveManager có trong scene
- Kiểm tra Console có lỗi về file path

## 📝 Các bước tiếp theo

Sau khi setup xong base:

1. **Thêm hiệu ứng:**
   - Particle khi vỡ block
   - Animation cho combo text
   - Tween cho block placement

2. **Thêm âm thanh:**
   - Sound effect cho các action
   - Background music

3. **Polish UI:**
   - Làm đẹp các panel
   - Thêm animation chuyển scene
   - Responsive design

4. **Optimize:**
   - Object pooling cho Cell/Block
   - Reduce garbage collection
   - Profile performance

## 💡 Tips

- Sử dụng Gizmos để debug vị trí board và spawn points
- Test trên nhiều resolution khác nhau
- Backup project thường xuyên
- Commit code vào Git sau mỗi tính năng hoàn thành

---
**Chúc bạn code game thành công! 🎮**
