# Block Blast - Kiến trúc hệ thống

## Tổng quan kiến trúc

```
GameManager (Controller chính)
    ├── BoardManager (Quản lý bảng 8x8)
    │   └── Cell[] (64 ô)
    ├── BlockSpawner (Sinh block)
    │   └── Block[] (3 blocks)
    ├── ScoreManager (Quản lý điểm)
    ├── SaveManager (Lưu/Load)
    ├── BlockDragHandler (Xử lý drag & drop)
    └── UIManager (Giao diện)
```

## Luồng xử lý chính

### 1. Khởi tạo Game
```
GameManager.Start()
  → Initialize tất cả managers
  → Subscribe events
  → Check save file
    → Có: Hiện menu (New/Load)
    → Không: Bắt đầu game mới
```

### 2. Bắt đầu game mới
```
StartNewGame()
  → BoardManager.ClearBoard()
  → ScoreManager.ResetScore()
  → BlockSpawner.SpawnBlocks() (tạo 3 blocks)
```

### 3. Người chơi kéo block
```
Input.GetMouseButtonDown()
  → BlockDragHandler phát hiện block được chọn
  → Block.OnMouseDown() → isDragging = true
  → Block.OnMouseDrag() → Di chuyển theo chuột
```

### 4. Người chơi thả block
```
Input.GetMouseButtonUp()
  → BlockDragHandler.HandleDrop()
  → Lấy vị trí grid từ mouse position
  → BoardManager.CanPlaceBlock(shape, position)
    → TRUE:
      ├── Animate block đến vị trí
      ├── BoardManager.PlaceBlock()
      ├── GameManager.OnBlockPlaced()
      │   ├── ScoreManager.AddScoreForPlacedBlock()
      │   ├── BoardManager.CheckAndClearLines()
      │   ├── ScoreManager.AddScoreForClearedLines()
      │   └── SaveManager.AutoSave()
      └── Block.PlaceSuccess() → ẩn block
    → FALSE:
      └── Animate block quay về vị trí ban đầu
```

### 5. Kiểm tra xóa hàng/cột
```
BoardManager.CheckAndClearLines()
  → Duyệt 8 hàng ngang
  → Duyệt 8 cột dọc
  → Nếu đầy:
    ├── Xóa các cell
    ├── Trigger OnRowsCleared / OnColumnsCleared
    └── Return danh sách hàng/cột đã xóa
```

### 6. Tính điểm và combo
```
ScoreManager.AddScoreForClearedLines(lineCount)
  → Nếu lineCount > 0:
    ├── Tính score = 8 * lineCount * (1 + combo * 0.1)
    ├── currentCombo++
    ├── movesSinceLastScore = 0
    └── Cập nhật best score nếu cần
  → Nếu lineCount == 0:
    ├── movesSinceLastScore++
    └── Nếu movesSinceLastScore >= 3 → combo = 0
```

### 7. Sinh block mới
```
BlockSpawner.OnAllBlocksPlaced event
  → GameManager.OnAllBlocksPlaced()
  → BlockSpawner.SpawnBlocks() (tạo 3 blocks mới)
  → Delay 0.5s → CheckGameOver()
```

### 8. Kiểm tra Game Over
```
CheckGameOver()
  → Lấy danh sách 3 blocks hiện tại
  → BoardManager.CanPlaceAnyBlock(blocks)
    → Duyệt qua tất cả vị trí trên board
    → Nếu có ít nhất 1 block đặt được → Continue
    → Nếu không có block nào đặt được → GameOver()
```

### 9. Game Over
```
GameOver()
  → UIManager.ShowGameOver(finalScore)
  → SaveManager.DeleteSaveFile()
  → isGameOver = true
```

## Event System

### BoardManager Events
- `OnCellFilled(x, y)` - Khi 1 ô được lấp đầy
- `OnRowsCleared(List<int>)` - Khi các hàng bị xóa
- `OnColumnsCleared(List<int>)` - Khi các cột bị xóa

### BlockSpawner Events
- `OnAllBlocksPlaced()` - Khi đã đặt hết 3 blocks

### ScoreManager Events
- `OnScoreChanged(int)` - Khi điểm thay đổi
- `OnBestScoreChanged(int)` - Khi best score thay đổi
- `OnComboChanged(int)` - Khi combo thay đổi

### BlockDragHandler Events
- `OnBlockPlacedSuccessfully(Block, Vector2Int)` - Khi block được đặt thành công

### Block Events
- `OnBlockPlaced(Block)` - Khi block được đặt xong

## Công thức tính điểm

### Điểm đặt block
```
score = cellCount * baseScorePerCell
Ví dụ: Block 2x2 = 4 ô = 4 điểm
```

### Điểm xóa hàng/cột
```
baseScore = lineCount * baseScorePerLine
            (1 hàng/cột = 8 điểm)

Với combo:
finalScore = baseScore * (1 + combo * comboMultiplier)
           = baseScore * (1 + combo * 0.1)

Ví dụ:
- Xóa 1 hàng, combo 0: 8 * (1 + 0*0.1) = 8
- Xóa 1 hàng, combo 1: 8 * (1 + 1*0.1) = 8.8 → 9
- Xóa 2 hàng, combo 3: 16 * (1 + 3*0.1) = 20.8 → 21
```

### Combo system
```
Ghi điểm trong nước đi hiện tại → combo++
Không ghi điểm:
  - movesSinceLastScore++
  - Nếu movesSinceLastScore >= 3 → combo = 0
```

## Lưu/Load Game

### Dữ liệu được lưu (GameData)
- `currentScore` - Điểm hiện tại
- `bestScore` - Điểm cao nhất
- `currentCombo` - Combo hiện tại
- `movesSinceLastScore` - Số nước đi kể từ lần ghi điểm cuối
- `boardState[8,8]` - Trạng thái bảng (0/1)
- `currentBlocks[]` - Thông tin 3 blocks hiện tại

### Thời điểm lưu
1. Sau mỗi nước đi (1 giây delay)
2. Khi thoát app (OnApplicationQuit)
3. Khi pause app (OnApplicationPause) - Mobile

### Thời điểm load
1. Khi bắt đầu game (nếu có save file)
2. Khi chọn "Continue" trong menu

## Tối ưu hiệu năng

### Object Pooling (Khuyến nghị)
- Cell objects (64 cells)
- Block objects (3 blocks * nhiều lần spawn)
- Particle effects

### Caching
- Camera.main → Cache trong Awake
- GetComponent → Cache trong Start
- Transform references

### Reduce GC
- Tránh tạo new object trong Update
- Sử dụng StringBuilder cho string operations
- Pre-allocate arrays/lists

## Mở rộng

### Thêm Block shapes mới
1. Mở [BlockShapeData.cs](BlockShapeData.cs)
2. Thêm pattern mới trong `InitializeShapes()`
```csharp
// Plus shape (+)
allShapes.Add(new BlockShape(new int[,] { 
    { 0, 1, 0 }, 
    { 1, 1, 1 }, 
    { 0, 1, 0 } 
}));
```

### Thêm Power-ups
1. Tạo class `PowerUp` kế thừa từ `MonoBehaviour`
2. Thêm vào `BlockShape` property `PowerUpType`
3. Xử lý trong `GameManager.OnBlockPlaced()`

### Thêm chế độ chơi mới
1. Tạo enum `GameMode`
2. Tạo class `GameModeConfig : GameConfig`
3. Override methods trong `GameManager`

### Multiplayer
1. Tạo `NetworkManager`
2. Sync `boardState` qua network
3. Lock input khi đang sync

---
**Updated:** Theo GDD.txt requirements
