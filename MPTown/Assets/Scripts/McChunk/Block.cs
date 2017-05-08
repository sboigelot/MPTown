namespace Assets.Scripts.McChunk
{
    public class Block
    {
        public bool empty;

        public byte id = 0;

        public Block(bool isEmpty)
        {
            empty = isEmpty;
        }

        public Block ReturnBlock
        {
            get { return this; }
        }
    }
}