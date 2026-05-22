Trong thư mục Forms:
Tầng			Vai trò
Forms			GUI
Models			Dữ liệu
Network			TCP/IP
Services		Logic
Shared			Protocol dùng chung
Utils			Hàm hỗ trợ

Form_Statup.cs
Nhiệm vụ:
- Chọn Client Mode
- Chọn Server Mode
- Mở form tương ứng
 Nó liên kết với:
Giai đoạn        Vai trò 
 4	           GUI Realtime System
 10	           Multi-PC Deployment

TCP_Chat_Client.cs
Đây là Client GUI, nhiệm vụ chính:
- Nhập IP
- Nhập Port
- Connect Server
- Hiển thị chat
- Gửi message
- Hiển thị online users
- Hiển thị rooms
Liên kết với:

Giai đoạn				Vai trò 
1				Connect TCP
2				Login Username
3				Join Room
4				GUI realtime
5				Send file
7				Typing indicator

TCP_Chat_Server.cs
đây là Server GUI
Nhiệm vụ:
- Start Server
- Stop Server
- Hiển thị logs
- Hiển thị client online
- Hiển thị rooms
Liên kết với:
Giai đoạn			Vai trò
1                 TCP Server
2                 User online
3                 Room Monifor
9                 Stress Test

Trong thư mục Model:
Đây là tầng data
UserModel.cs:
vai trò như là đại diện người dùng
liên kết:
Giai đoạn			Vai trò
2				User Management
3				Room membership
7				User status

MessageModel.cs
Đại diện cho tin nhắn, là file quan trọng vì mọi tính năng đều thông qua nó
Liên kết:
Giai đoạn			Vai trò
1				Message TCP
3				Room Message
5				File message
8				Save history

PacketModel.cs
là Protocol object
liên kết:
Giai đoạn			Vai trò
1				TCP protocol
2				Login
3				Room_MSG
5				File_Transfer

Trong thư mục Network:
ChatClientManager.cs
đây là TCP Client Egine, nhiệm vụ chính:
- Connect server
- Send packet
- Receive packet
- Handle NetworkStream
Liên kết:								
Giai đoạn           Vai trò
1              TCP Client
4              Background recevie
5              File send

TCPChatServer.cs
Đây là TCP Server Engine
Nhiệm vụ:
- Open Port
- Accept Client
- Create ClientHandler
- Manage Connections
Liên kết:		
Giai đoạn          Vai trò
1              TCP Server
1.5          Multi-Client
9              Stress test

ClientHandler.cs
Đây là Client Session Thread
Vai trò: Mỗi Client sẽ có 1 ClientHandler riêng
Nhiệm vụ:
- Read packet
- Parse packet
- Detect disconnect
- Broadcast
Liên kết:		
Giai đoạn          Vai trò
1.5             Multi-Thread
2               User identity
3               Room Routing
7               Realtime feture

Trong thư mục Service:
Đây là Business Logic Layer
MessageServvice.cs
đóng vai trò xử lý Logic Message
Liên kết:
Giai đoạn         Vai trò
1				Send Message
7				Timestamp
9				Spam test

UIService.cs
Đây là GUI Helper Layer
Vai trò:
- Append chat
- Update user list
- Update room list
- Thread-safe invoke
Quan trọng trong WinForms vì Network thread != UI thread
Liên kết:
Giai đoạn        Vai trò
4              Thread-Safe GUI
7              Notification

Trong thư mục Shared:
Đây là Protocol Player
PacketTypes.cs
Đây là giao thức hệ thống
Liên kết
Giai đoạn			Vai trò
1				TCP Pacet
2				login
3				Room
5				File_Transfer

PacketBuilder.cs
Đây là packet creator, Liên kết:
Giai đoạn			Vai trò
1				Message framing
3				Room Packet

PacketParser.cs
Đây là packet analyzer, Liên kết:
Giai đoạn			Vai trò
1				packet parse
2				Login parse
3				Room parse

EncodingHelper.cs
Đây là byte encoding helper, liên kết:
Giai đoạn			Vai trò
1				TCP bytes
5				File bytes
6				Encryption

Trong thư mục Ultils:
Đây là helper layer, Liên kết:
Giai đoạn			Vai trò
4				GUI enhancement
7				Notification UI




	
	
	
	
	

