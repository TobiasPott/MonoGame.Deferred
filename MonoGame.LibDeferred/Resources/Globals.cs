using Microsoft.Xna.Framework.Content;

namespace DeferredEngine.Recources
{
    //As suggested here http://community.monogame.net/t/deferred-engine-playground-download/8180/283?u=kosmonautgames
    //the whole global shaders is shortened to load early without the need for a seperate load function
    // by bettina4you

    public static class Globals
    {
        public static ContentManager content;
    }

}
