using System;

namespace Assets.Scripts.DTO
{
    [Serializable]
    public class ClientDTO
    {
        public int id;

        public string name;

        public override bool Equals(object obj)
        {
            if(this.id == (obj as ClientDTO).id)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}
