# Block Blast Game - Unity Project

## Mô tả dự án
Đây là game puzzle Block Blast được phát triển trên Unity. Người chơi kéo thả các khối block lên bảng 8x8, xóa hàng/cột để ghi điểm.

## Cấu trúc dự án

### 📁 Scripts/Data
Chứa các class dữ liệu và cấu hình:
- **BlockShape.cs**: Định nghĩa hình dạng khối block
- **BlockShapeData.cs**: Chứa tất cả các hình dạng block có thể xuất hiện
- **GameConfig.cs**: ScriptableObject cấu hình game (kích thước bảng, điểm số, combo...)
- **GameData.cs**: Dữ liệu game để lưu/load

### 📁 Scripts/Core
Chứa logic game chính:
- **GameManager.cs**: Quản lý toàn bộ flow game
- **BoardManager.cs**: Quản lý bảng chơi 8x8
- **Cell.cs**: Đại diện cho một ô trên bảng
- **BlockSpawner.cs**: Sinh và quản lý các block
- **Block.cs**: Class đại diện cho một khối block
- **BlockDragHandler.cs**: Xử lý drag & drop block
- **ScoreManager.cs**: Quản lý điểm số và combo
- **SaveManager.cs**: Quản lý lưu/load game

### 📁 Scripts/UI
Quản lý giao diện:
- **UIManager.cs**: Quản lý UI chính
- **ComboEffect.cs**: Hiệu ứng hiển thị combo

### 📁 Scripts/Effects
Hiệu ứng đặc biệt:
- **CellDestroyEffect.cs**: Hiệu ứng vỡ khi xóa ô

### 📁 Scripts/Utils
Các tiện ích:
- **GameUtils.cs**: Hàm tiện ích chung
- **AudioManager.cs**: Quản lý âm thanh

## Cách setup

### Bước 1: Tạo GameConfig ScriptableObject
1. Click phải trong Project → Create → Block Blast → Game Config
2. Đặt tên "GameConfig"
3. Cấu hình các thông số:
   - Board Width: 8
   - Board Height: 8
   - Block Spawn Count: 3
   - Cell Size: 1
   - Base Score Per Cell: 1
   - Base Score Per Line: 8
   - Combo Multiplier: 0.1
4. **Assign Sprites từ Graphics folder:**
   - Click button "Auto-Assign Stone Sprites from Graphics Folder"
   - Click button "Auto-Assign UI Sprites from Graphics Folder"
   - Hoặc assign thủ công các stone sprites (blueStone, redStone, etc.)

### Bước 2: Tạo Prefabs

#### Cell Prefab
1. Tạo GameObject mới, đặt tên "Cell"
2. Add component: SpriteRenderer
3. Add component: Cell.cs
4. Tạo sprite vuông đơn giản (hoặc sử dụng Unity's Default-Sprite)
5. Lưu thành prefab

#### Block Prefab
1. Tạo GameObject mới, đặt tên "Block"
2. Add component: Block.cs
3. Add component: BoxCollider2D (để detect mouse)
4. Lưu thành prefab

### Bước 3: Setup Scene

#### Tạo Board
1. Tạo Empty GameObject "Board"
2. Add component: BoardManager.cs
3. Assign Cell Prefab vào BoardManager

#### Tạo Block Spawner
1. Tạo Empty GameObject "BlockSpawner"
2. Add component: BlockSpawner.cs
3. Tạo 3 Empty GameObject con làm spawn positions (đặt ở dưới màn hình)
4. Assign Block Prefab và spawn positions vào BlockSpawner

#### Tạo Game Manager
1. Tạo Empty GameObject "GameManager"
2. Add các components:
   - GameManager.cs
   - ScoreManager.cs
   - SaveManager.cs
   - BlockDragHandler.cs
3. Assign tất cả references cần thiết

#### Setup UI
1. Tạo Canvas
2. Tạo các Text elements cho Score, Best Score, Combo
3. Tạo Game Over Panel với Restart button
4. Tạo Menu Panel với New Game và Load Game buttons
5. Add UIManager.cs vào Canvas
6. Assign tất cả UI references

### Bước 4: Setup Camera
- Đặt Camera ở vị trí nhìn thẳng xuống board
- Camera Size: 6-8 (tùy thiết kế)
- Background: Màu tối

## Gameplay Logic

### Luật chơi
1. **Đặt Block**: Kéo block từ dưới lên bảng, không được đè lên nhau
2. **Ghi điểm**: 
   - Đặt block: +1 điểm/ô
   - Xóa hàng/cột: 8 điểm (có combo)
3. **Combo**: 
   - Ghi điểm trong 3 nước liên tiếp → combo tăng
   - Công thức: `8 * (1 + combo * 0.1)`
   - Không ghi điểm trong 3 nước → combo về 0
4. **Game Over**: Không còn chỗ đặt block nào

### Flow Game
1. Game bắt đầu → Bảng trống, sinh 3 block
2. Người chơi drag-drop block lên bảng
3. Kiểm tra và xóa hàng/cột đầy
4. Tính điểm và combo
5. Khi đặt hết 3 block → Sinh 3 block mới
6. Kiểm tra game over
7. Auto save sau mỗi nước đi

## Features đã implement

✅ Logic game cơ bản (drag, drop, spawn, score)  
✅ Hệ thống combo  
✅ Lưu/Load game  
✅ Best score  
✅ UI cơ bản  
✅ Game over detection  
✅ Auto save  

## Features cần hoàn thiện

🔲 Hiệu ứng vỡ block (CellDestroyEffect đã có skeleton)  
🔲 Animation combo text  
🔲 Sound effects  
🔲 Particle effects  
🔲 Polish UI  
🔲 Tutorial  
🔲 Settings menu  

## Notes cho Developer

### Tối ưu
- Sử dụng Object Pooling cho Cell và Block prefabs
- Cache các component references
- Tránh FindObjectOfType trong Update()

### Mở rộng
- Có thể thêm power-ups
- Thêm chế độ chơi khác (time attack, challenge...)
- Leaderboard
- Daily rewards

### Bug cần fix
- Kiểm tra edge case khi drag block ra ngoài screen
- Validate vị trí đặt block kỹ hơn
- Xử lý multi-touch trên mobile

## Liên hệ
Dự án được tạo theo Game Design Document (GDD.txt)
