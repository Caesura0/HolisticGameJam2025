public class ConsumableItem : PickableItem
{
    public void Consume()
    {
        if (!initialized)
            return;

        transform.gameObject.SetActive(false);
        Release();
    }
}
