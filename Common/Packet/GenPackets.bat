START ../../PacketGenerator/bin/Debug/net6.0/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
rem /Y는 같은 이름의 파일이 있으면 덮어쓴다는 옵션
XCOPY /Y GenPackets.cs "../../Server/Packet"
rem 빌드하는 순간에 자동으로 batch 파일을 실행해 GenPackets을 생성하게 할 수도 있겠다.