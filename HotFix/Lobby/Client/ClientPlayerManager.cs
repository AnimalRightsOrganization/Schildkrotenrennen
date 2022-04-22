namespace HotFix
{
    // ֻ�������ڵ����
    public class ClientPlayerManager
    {
        public ClientPlayerManager() { }

        private ClientPlayer _localPlayer; //�Լ�
        public ClientPlayer LocalPlayer => _localPlayer;

        // ����
        public void Reset()
        {
            _localPlayer = null;
        }

        // ������¼�ɹ���ƥ��ɹ�������
        public void AddClientPlayer(ClientPlayer player, bool isSelf)
        {
            if (isSelf)
                _localPlayer = player;

            player.ResetToLobby();
        }
    }
}