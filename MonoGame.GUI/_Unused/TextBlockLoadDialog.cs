﻿//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using MonoGame.GUIHelper;
//using System.Reflection;
//using System.Text;

//namespace MonoGame.GUI
//{
//    public class TextBlockLoadDialog : TextBlock
//    {
//        public bool Toggle;

//        private static readonly float ButtonBorder = 2;

//        private static readonly Color HoverColor = Color.LightGray;

//        private static readonly int HoverImageWidth = 250;

//        private Vector2 _declarationTextDimensions;

//        private bool _isHovered;

//        private short _isLoaded; //0 -> 1 -> 2

//        //Load
//        private Task _loadTaskReference;
//        public object LoadedObject;
//        private int _loadedObjectPointer = -1;
//        private readonly StringBuilder _loadedObjectName = new StringBuilder(100);
//        private readonly StringBuilder _loadingStringBuilder = new StringBuilder("loading...");

//        public MethodInfo LoaderMethod;
//        public GUIContentLoader GUILoader;

//        public enum ContentType
//        {
//            Texture2D
//        };

//        public TextBlockLoadDialog(GUIStyle style, string text, GUIContentLoader contentLoader, ContentType contentType) : this(
//            position: Vector2.Zero,
//            dimensions: style.Dimensions,
//            text: text,
//            guiContentLoader: contentLoader,
//            contentType: contentType,
//            font: style.TextFont,
//            blockColor: style.Color,
//            textColor: style.TextColor,
//            textAlignment: TextAlignment.Left,
//            textBorder: style.TextBorder,
//            layer: 0)
//        {
//        }
//        public TextBlockLoadDialog(Vector2 position, Vector2 dimensions, string text, GUIContentLoader guiContentLoader, ContentType contentType, SpriteFont font, Color blockColor, Color textColor, TextAlignment textAlignment = TextAlignment.Center, Vector2 textBorder = default, int layer = 0) : base(position, dimensions, text, font, blockColor, textColor, textAlignment, textBorder, layer)
//        {
//            _loadedObjectName.Append("...");

//            //Initialize the loader and the kind of content we want to retrieve

//            GUILoader = guiContentLoader;

//            Type type = null;
//            switch (contentType)
//            {
//                case ContentType.Texture2D:
//                    type = typeof(Texture2D);
//                    break;
//            }

//            LoaderMethod = GUILoader.GetType().GetMethod("LoadContentFile").MakeGenericMethod(type);
//        }

//        protected override void ComputeFontPosition()
//        {
//            if (_text == null) return;
//            _declarationTextDimensions = TextFont.MeasureString(_text);

//            //Let's check wrap!

//            //FontWrap(ref textDimension, Dimensions);

//            _fontPosition = Dimensions * 0.5f * Vector2.UnitY + _textBorder * Vector2.UnitX - _declarationTextDimensions * 0.5f * Vector2.UnitY;
//        }

//        protected void ComputeObjectNameLength()
//        {
//            if (_loadedObjectName.Length > 0)
//            {
//                //Max length
//                Vector2 textDimensions = TextFont.MeasureString(_loadedObjectName);

//                float characterLength = textDimensions.X / _loadedObjectName.Length;

//                Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.5f) * Vector2.UnitX;
//                Vector2 spaceAvailable = Dimensions - 2 * Vector2.One * ButtonBorder - buttonLeft -
//                                         (2 + _textBorder.X) * Vector2.UnitX;

//                int characters = (int)(spaceAvailable.X / characterLength);

//                _loadedObjectName.Length = characters < _loadedObjectName.Length ? characters : _loadedObjectName.Length;
//            }
//        }


//        public override void Draw(GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
//        {
//            Vector2 buttonLeft = (_declarationTextDimensions + _fontPosition * 1.2f) * Vector2.UnitX;
//            guiRenderer.DrawQuad(parentPosition + Position, Dimensions, SwatchColor);
//            guiRenderer.DrawQuad(parentPosition + Position + buttonLeft + Vector2.One * ButtonBorder, Dimensions - 2 * Vector2.One * ButtonBorder - buttonLeft - (2 + _textBorder.X) * Vector2.UnitX, _isHovered ? HoverColor : Color.DimGray);

//            Vector2 indicatorButton = parentPosition + new Vector2(Dimensions.X - (2 + _textBorder.X), Dimensions.Y / 2 - 4);

//            guiRenderer.DrawQuad(indicatorButton, Vector2.One * 8, _isLoaded < 1 ? Color.Red : (_isLoaded < 2 ? Color.Yellow : Color.LimeGreen));

//            guiRenderer.DrawText(parentPosition + Position + _fontPosition, Text, TextFont, TextColor);

//            //Description
//            guiRenderer.DrawText(parentPosition + Position + buttonLeft + new Vector2(4, _fontPosition.Y), _isLoaded == 1 ? _loadingStringBuilder : _loadedObjectName, TextFont, TextColor);

//            //Show texture if _isHovered
//            if (_isLoaded == 2)
//            {
//                LoadedObject = GUILoader.ContentArray[_loadedObjectPointer];

//                if (_isHovered)
//                {
//                    //compute position

//                    Vector2 position = mousePosition;

//                    float overborder = position.X + HoverImageWidth - GUIMouseInput.ScreenWidth;

//                    if (overborder > 0)
//                        position.X -= overborder;


//                    if (LoadedObject != null && LoadedObject.GetType() == typeof(Texture2D))
//                    {
//                        Texture2D image = (Texture2D)LoadedObject;
//                        float height = (float)image.Height / image.Width * HoverImageWidth;
//                        guiRenderer.DrawImage(position, new Vector2(HoverImageWidth, height),
//                            image, Color.White, true);
//                    }
//                }
//            }

//        }

//        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
//        {
//            if (_loadTaskReference != null)
//            {
//                _isLoaded = (short)(_loadTaskReference.IsCompleted ? 2 : 1);

//                if (_isLoaded == 2)
//                {
//                    if (_loadTaskReference.IsFaulted)
//                    {
//                        _isLoaded = 0;
//                        _loadedObjectName.Clear();
//                        _loadedObjectName.Append("Loading failed");
//                    }


//                }
//            }
//            else
//            {
//                _isLoaded = 0;
//            }

//            _isHovered = false;

//            Vector2 bound1 = Position + parentPosition;
//            Vector2 bound2 = bound1 + Dimensions;

//            if (mousePosition.X >= bound1.X && mousePosition.Y >= bound1.Y && mousePosition.X < bound2.X &&
//                mousePosition.Y < bound2.Y)
//            {
//                _isHovered = true;

//                if (!GUIMouseInput.WasLMBClicked()) return;

//                GUIMouseInput.UIWasUsed = true;

//                if (GUILoader != null)
//                {
//                    string s = null;
//                    object[] args = { _loadTaskReference, _loadedObjectPointer, s };
//                    LoaderMethod?.Invoke(GUILoader, args);

//                    _loadTaskReference = (Task)args[0];
//                    _loadedObjectPointer = (int)args[1];
//                    _loadedObjectName.Clear();
//                    _loadedObjectName.Append((string)args[2]);

//                    ComputeObjectNameLength();
//                }
//            }
//        }

//    }

//}