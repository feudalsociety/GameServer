START ../../PacketGenerator/bin/Debug/net6.0/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
rem /Y�� ���� �̸��� ������ ������ ����ٴ� �ɼ�
XCOPY /Y GenPackets.cs "../../Server/Packet"
rem �����ϴ� ������ �ڵ����� batch ������ ������ GenPackets�� �����ϰ� �� ���� �ְڴ�.