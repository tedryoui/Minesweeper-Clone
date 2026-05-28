using _.Scripts.Services;

namespace _.Scripts.User_Interface
{
    public class LoseUserInterface : AbstractUserInterface
    {
        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}