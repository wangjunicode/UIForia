using System;
using System.Collections.Generic;
using System.Globalization;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

// ReSharper disable StringLiteralTypo

namespace UIForia.Compilers.Style {

    public static class StylePropertyMappers {

        public const string k_RepeatFit = "fit";
        public const string k_RepeatFill = "fill";

        private static readonly Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> mappers
            = new Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> {
                // Overflow
                {"overflow", (targetStyle, property, context) => MapOverflows(targetStyle, property, context)},
                {"overflowx", (targetStyle, property, context) => targetStyle.OverflowX = MapEnum<Overflow>(property.children[0], context)},
                {"overflowy", (targetStyle, property, context) => targetStyle.OverflowY = MapEnum<Overflow>(property.children[0], context)},
                {"clipbehavior", (targetStyle, property, context) => targetStyle.ClipBehavior = MapEnum<ClipBehavior>(property.children[0], context)},
                {"clipbounds", (targetStyle, property, context) => targetStyle.ClipBounds = MapEnum<ClipBounds>(property.children[0], context)},

                // Alignment
                {"alignmentbehaviorx", (targetStyle, property, context) => targetStyle.AlignmentBehaviorX = MapEnum<AlignmentBehavior>(property.children[0], context)},
                {"alignmentbehaviory", (targetStyle, property, context) => targetStyle.AlignmentBehaviorY = MapEnum<AlignmentBehavior>(property.children[0], context)},
                {"alignmentbehavior", (targetStyle, property, context) => {
                        if (property.children.size == 1) {
                            AlignmentBehavior target = MapEnum<AlignmentBehavior>(property.children[0], context);
                            targetStyle.AlignmentBehaviorX = target;
                            targetStyle.AlignmentBehaviorY = target;
                        }
                        else {
                            targetStyle.AlignmentBehaviorX = MapEnum<AlignmentBehavior>(property.children[0], context);
                            targetStyle.AlignmentBehaviorY = MapEnum<AlignmentBehavior>(property.children[1], context);
                        }
                    }
                },
                {"alignmentoriginx", (targetStyle, property, context) => targetStyle.AlignmentOriginX = MapOffsetMeasurement(property.children[0], context)},
                {"alignmentoriginy", (targetStyle, property, context) => targetStyle.AlignmentOriginY = MapOffsetMeasurement(property.children[0], context)},
                {"alignmentoffsetx", (targetStyle, property, context) => targetStyle.AlignmentOffsetX = MapOffsetMeasurement(property.children[0], context)},
                {"alignmentoffsety", (targetStyle, property, context) => targetStyle.AlignmentOffsetY = MapOffsetMeasurement(property.children[0], context)},
                {"alignmentdirectionx", (targetStyle, property, context) => targetStyle.AlignmentDirectionX = MapEnum<AlignmentDirection>(property.children[0], context)},
                {"alignmentdirectiony", (targetStyle, property, context) => targetStyle.AlignmentDirectionY = MapEnum<AlignmentDirection>(property.children[0], context)},
                {"alignx", (targetStyle, property, context) => MapAlignmentX(targetStyle, property, context)},
                {"aligny", (targetStyle, property, context) => MapAlignmentY(targetStyle, property, context)},

                {"layoutfit", (targetStyle, property, context) => MapLayoutFit(targetStyle, property, context)},
                {"layoutfithorizontal", (targetStyle, property, context) => targetStyle.LayoutFitHorizontal = MapEnum<LayoutFit>(property.children[0], context)},
                {"layoutfitvertical", (targetStyle, property, context) => targetStyle.LayoutFitVertical = MapEnum<LayoutFit>(property.children[0], context)},

                // Background
                {"backgroundcolor", (targetStyle, property, context) => targetStyle.BackgroundColor = MapColor(property, context)},
                {"backgroundtint", (targetStyle, property, context) => targetStyle.BackgroundTint = MapColor(property, context)},
                {"backgroundimageoffsetx", (targetStyle, property, context) => targetStyle.BackgroundImageOffsetX = MapFixedLength(property.children[0], context)},
                {"backgroundimageoffsety", (targetStyle, property, context) => targetStyle.BackgroundImageOffsetY = MapFixedLength(property.children[0], context)},
                {"backgroundimagescalex", (targetStyle, property, context) => targetStyle.BackgroundImageScaleX = MapNumber(property.children[0], context)},
                {"backgroundimagescaley", (targetStyle, property, context) => targetStyle.BackgroundImageScaleY = MapNumber(property.children[0], context)},
                {"backgroundimagetilex", (targetStyle, property, context) => targetStyle.BackgroundImageTileX = MapNumber(property.children[0], context)},
                {"backgroundimagetiley", (targetStyle, property, context) => targetStyle.BackgroundImageTileY = MapNumber(property.children[0], context)},
                {"backgroundimagerotation", (targetStyle, property, context) => targetStyle.BackgroundImageRotation = MapNumber(property.children[0], context)},
                {"backgroundimage", (targetStyle, property, context) => targetStyle.BackgroundImage = MapTexture(property.children[0], context)},
                {"backgroundfit", (targetStyle, property, context) => targetStyle.BackgroundFit = MapEnum<BackgroundFit>(property.children[0], context)},

                {"visibility", (targetStyle, property, context) => targetStyle.Visibility = MapEnum<Visibility>(property.children[0], context)},
                {"opacity", (targetStyle, property, context) => targetStyle.Opacity = MapNumber(property.children[0], context)},
                {"cursor", (targetStyle, property, context) => targetStyle.Cursor = MapCursor(property, context)},

                {"margin", (targetStyle, valueParts, context) => MapMargins(targetStyle, valueParts, context)},
                {"margintop", (targetStyle, property, context) => targetStyle.MarginTop = MapFixedLength(property.children[0], context)},
                {"marginright", (targetStyle, property, context) => targetStyle.MarginRight = MapFixedLength(property.children[0], context)},
                {"marginbottom", (targetStyle, property, context) => targetStyle.MarginBottom = MapFixedLength(property.children[0], context)},
                {"marginleft", (targetStyle, property, context) => targetStyle.MarginLeft = MapFixedLength(property.children[0], context)},

                {"padding", (targetStyle, valueParts, context) => MapPaddings(targetStyle, valueParts, context)},
                {"paddingtop", (targetStyle, property, context) => targetStyle.PaddingTop = MapFixedLength(property.children[0], context)},
                {"paddingright", (targetStyle, property, context) => targetStyle.PaddingRight = MapFixedLength(property.children[0], context)},
                {"paddingbottom", (targetStyle, property, context) => targetStyle.PaddingBottom = MapFixedLength(property.children[0], context)},
                {"paddingleft", (targetStyle, property, context) => targetStyle.PaddingLeft = MapFixedLength(property.children[0], context)},

                {"bordercolor", (targetStyle, property, context) => MapBorderColors(targetStyle, property, context)},
                {"bordercolortop", (targetStyle, property, context) => targetStyle.BorderColorTop = MapColor(property, context)},
                {"bordercolorright", (targetStyle, property, context) => targetStyle.BorderColorRight = MapColor(property, context)},
                {"bordercolorbottom", (targetStyle, property, context) => targetStyle.BorderColorBottom = MapColor(property, context)},
                {"bordercolorleft", (targetStyle, property, context) => targetStyle.BorderColorLeft = MapColor(property, context)},

                {"border", (targetStyle, property, context) => MapBorders(targetStyle, property, context)},
                {"bordertop", (targetStyle, property, context) => targetStyle.BorderTop = MapFixedLength(property.children[0], context)},
                {"borderright", (targetStyle, property, context) => targetStyle.BorderRight = MapFixedLength(property.children[0], context)},
                {"borderbottom", (targetStyle, property, context) => targetStyle.BorderBottom = MapFixedLength(property.children[0], context)},
                {"borderleft", (targetStyle, property, context) => targetStyle.BorderLeft = MapFixedLength(property.children[0], context)},

                {"borderradius", (targetStyle, property, context) => MapBorderRadius(targetStyle, property, context)},
                {"borderradiustopleft", (targetStyle, property, context) => targetStyle.BorderRadiusTopLeft = MapFixedLength(property.children[0], context)},
                {"borderradiustopright", (targetStyle, property, context) => targetStyle.BorderRadiusTopRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomright", (targetStyle, property, context) => targetStyle.BorderRadiusBottomRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomleft", (targetStyle, property, context) => targetStyle.BorderRadiusBottomLeft = MapFixedLength(property.children[0], context)},

                {"cornerbeveltopleft", (targetStyle, property, context) => targetStyle.CornerBevelTopLeft = MapFixedLength(property.children[0], context)},
                {"cornerbeveltopright", (targetStyle, property, context) => targetStyle.CornerBevelTopRight = MapFixedLength(property.children[0], context)},
                {"cornerbevelbottomright", (targetStyle, property, context) => targetStyle.CornerBevelBottomRight = MapFixedLength(property.children[0], context)},
                {"cornerbevelbottomleft", (targetStyle, property, context) => targetStyle.CornerBevelBottomLeft = MapFixedLength(property.children[0], context)},

                {"griditemx", (targetStyle, property, context) => targetStyle.GridItemX = MapGridItemPlacement(property, context)},
                {"griditemy", (targetStyle, property, context) => targetStyle.GridItemY = MapGridItemPlacement(property, context)},
                {"griditemwidth", (targetStyle, property, context) => targetStyle.GridItemWidth = MapGridItemPlacement(property, context)},
                {"griditemheight", (targetStyle, property, context) => targetStyle.GridItemHeight = MapGridItemPlacement(property, context)},
                {"gridlayoutcolalignment", (targetStyle, property, context) => targetStyle.GridLayoutColAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutrowalignment", (targetStyle, property, context) => targetStyle.GridLayoutRowAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutdensity", (targetStyle, property, context) => targetStyle.GridLayoutDensity = MapEnum<GridLayoutDensity>(property.children[0], context)},
                {"gridlayoutcoltemplate", (targetStyle, property, context) => targetStyle.GridLayoutColTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutrowtemplate", (targetStyle, property, context) => targetStyle.GridLayoutRowTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutdirection", (targetStyle, property, context) => targetStyle.GridLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"gridlayoutcolautosize", (targetStyle, property, context) => targetStyle.GridLayoutColAutoSize = MapGridLayoutTemplate(property, context)},
                {"gridlayoutrowautosize", (targetStyle, property, context) => targetStyle.GridLayoutRowAutoSize = MapGridLayoutTemplate(property, context)},
                {"gridlayoutcolgap", (targetStyle, property, context) => targetStyle.GridLayoutColGap = MapNumber(property.children[0], context)},
                {"gridlayoutrowgap", (targetStyle, property, context) => targetStyle.GridLayoutRowGap = MapNumber(property.children[0], context)},
                
                {"stacklayoutalignhorizontal", (targetStyle, property, context) => targetStyle.StackLayoutAlignHorizontal = MapRelativeValue(property.children[0], context)},
                {"stacklayoutalignvertical", (targetStyle, property, context) => targetStyle.StackLayoutAlignVertical = MapRelativeValue(property.children[0], context)},

                {"flexitemgrow", (targetStyle, property, context) => targetStyle.FlexItemGrow = (int) MapNumber(property.children[0], context)},
                {"flexitemshrink", (targetStyle, property, context) => targetStyle.FlexItemShrink = (int) MapNumber(property.children[0], context)},
                {"flexlayoutwrap", (targetStyle, property, context) => targetStyle.FlexLayoutWrap = MapEnum<LayoutWrap>(property.children[0], context)},
                {"flexlayoutdirection", (targetStyle, property, context) => targetStyle.FlexLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"flexlayoutmainaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutMainAxisAlignment = MapEnum<MainAxisAlignment>(property.children[0], context)},
                {"flexlayoutcrossaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutCrossAxisAlignment = MapEnum<CrossAxisAlignment>(property.children[0], context)},

                {"radiallayoutstartangle", (targetStyle, property, context) => targetStyle.RadialLayoutStartAngle = MapNumber(property.children[0], context)},
                {"radiallayoutendangle", (targetStyle, property, context) => targetStyle.RadialLayoutEndAngle = MapNumber(property.children[0], context)},
                {"radiallayoutradius", (targetStyle, property, context) => targetStyle.RadialLayoutRadius = MapFixedLength(property.children[0], context)},

                {"transformposition", (targetStyle, property, context) => MapTransformPosition(targetStyle, property, context)},
                {"transformpositionx", (targetStyle, property, context) => targetStyle.TransformPositionX = MapOffsetMeasurement(property.children[0], context)},
                {"transformpositiony", (targetStyle, property, context) => targetStyle.TransformPositionY = MapOffsetMeasurement(property.children[0], context)},
                {"transformscale", (targetStyle, property, context) => MapTransformScale(targetStyle, property, context)},
                {"transformscalex", (targetStyle, property, context) => targetStyle.TransformScaleX = MapNumber(property.children[0], context)},
                {"transformscaley", (targetStyle, property, context) => targetStyle.TransformScaleY = MapNumber(property.children[0], context)},
                {"transformpivot", (targetStyle, property, context) => MapTransformPivot(targetStyle, property, context)},
                {"transformpivotx", (targetStyle, property, context) => targetStyle.TransformPivotX = MapFixedLength(property.children[0], context)},
                {"transformpivoty", (targetStyle, property, context) => targetStyle.TransformPivotY = MapFixedLength(property.children[0], context)},
                {"transformrotation", (targetStyle, property, context) => targetStyle.TransformRotation = MapNumber(property.children[0], context)},
                {"transformbehavior", (targetStyle, property, context) => MapTransformBehavior(targetStyle, property, context)},
                {"transformbehaviorx", (targetStyle, property, context) => targetStyle.TransformBehaviorX = MapEnum<TransformBehavior>(property.children[0], context)},
                {"transformbehaviory", (targetStyle, property, context) => targetStyle.TransformBehaviorY = MapEnum<TransformBehavior>(property.children[0], context)},

                {"minwidth", (targetStyle, property, context) => targetStyle.MinWidth = MapMeasurement(property.children[0], context)},
                {"minheight", (targetStyle, property, context) => targetStyle.MinHeight = MapMeasurement(property.children[0], context)},
                {"preferredwidth", (targetStyle, property, context) => targetStyle.PreferredWidth = MapMeasurement(property.children[0], context)},
                {"preferredheight", (targetStyle, property, context) => targetStyle.PreferredHeight = MapMeasurement(property.children[0], context)},
                {"maxwidth", (targetStyle, property, context) => targetStyle.MaxWidth = MapMeasurement(property.children[0], context)},
                {"maxheight", (targetStyle, property, context) => targetStyle.MaxHeight = MapMeasurement(property.children[0], context)},
                {"preferredsize", (targetStyle, property, context) => MapPreferredSize(targetStyle, property, context)},
                {"minsize", (targetStyle, property, context) => MapMinSize(targetStyle, property, context)},
                {"maxsize", (targetStyle, property, context) => MapMaxSize(targetStyle, property, context)},

                {"layouttype", (targetStyle, property, context) => targetStyle.LayoutType = MapEnum<LayoutType>(property.children[0], context)},
                {"layoutbehavior", (targetStyle, property, context) => targetStyle.LayoutBehavior = MapEnum<LayoutBehavior>(property.children[0], context)},
                {"anchortarget", (targetStyle, property, context) => targetStyle.AnchorTarget = MapEnum<AnchorTarget>(property.children[0], context)},
                {"anchortop", (targetStyle, property, context) => targetStyle.AnchorTop = MapFixedLength(property.children[0], context)},
                {"anchorright", (targetStyle, property, context) => targetStyle.AnchorRight = MapFixedLength(property.children[0], context)},
                {"anchorbottom", (targetStyle, property, context) => targetStyle.AnchorBottom = MapFixedLength(property.children[0], context)},
                {"anchorleft", (targetStyle, property, context) => targetStyle.AnchorLeft = MapFixedLength(property.children[0], context)},
                {"zindex", (targetStyle, property, context) => targetStyle.ZIndex = (int) MapNumber(property.children[0], context)},
                {"renderlayer", (targetStyle, property, context) => targetStyle.RenderLayer = MapEnum<RenderLayer>(property.children[0], context)},
                {"renderlayeroffset", (targetStyle, property, context) => targetStyle.RenderLayerOffset = (int) MapNumber(property.children[0], context)},
                {"layer", (targetStyle, property, context) => targetStyle.Layer = (int) MapNumber(property.children[0], context)},

                // Text
                {"textcolor", (targetStyle, property, context) => targetStyle.TextColor = MapColor(property, context)},
                {"caretcolor", (targetStyle, property, context) => targetStyle.CaretColor = MapColor(property, context)},
                {"selectionbackgroundcolor", (targetStyle, property, context) => targetStyle.SelectionBackgroundColor = MapColor(property, context)},
                {"selectiontextcolor", (targetStyle, property, context) => targetStyle.SelectionTextColor = MapColor(property, context)},
                {"textfontasset", (targetStyle, property, context) => targetStyle.TextFontAsset = MapFont(property.children[0], context)},
                {"textfontstyle", (targetStyle, property, context) => targetStyle.TextFontStyle = MapTextFontStyle(property, context)},
                {"textfontsize", (targetStyle, property, context) => targetStyle.TextFontSize = MapFixedLength(property.children[0], context)},
                {"textfacedilate", (targetStyle, property, context) => targetStyle.TextFaceDilate = Mathf.Clamp01(MapNumber(property.children[0], context))},
                {"textalignment", (targetStyle, property, context) => targetStyle.TextAlignment = MapEnum<TextAlignment>(property.children[0], context)},
                {"textoutlinewidth", (targetStyle, property, context) => targetStyle.TextOutlineWidth = MapNumber(property.children[0], context)},
                {"textoutlinecolor", (targetStyle, property, context) => targetStyle.TextOutlineColor = MapColor(property, context)},
                {"textoutlinesoftness", (targetStyle, property, context) => targetStyle.TextOutlineSoftness = MapNumber(property.children[0], context)},
                {"textglowcolor", (targetStyle, property, context) => targetStyle.TextGlowColor = MapColor(property, context)},
                {"textglowoffset", (targetStyle, property, context) => targetStyle.TextGlowOffset = MapNumber(property.children[0], context)},
                {"textglowinner", (targetStyle, property, context) => targetStyle.TextGlowInner = MapNumber(property.children[0], context)},
                {"textglowouter", (targetStyle, property, context) => targetStyle.TextGlowOuter = MapNumber(property.children[0], context)},
                {"textglowpower", (targetStyle, property, context) => targetStyle.TextGlowPower = MapNumber(property.children[0], context)},
                {"textunderlaycolor", (targetStyle, property, context) => targetStyle.TextUnderlayColor = MapColor(property, context)},
                {"textunderlayx", (targetStyle, property, context) => targetStyle.TextUnderlayX = MapNumber(property.children[0], context)},
                {"textunderlayy", (targetStyle, property, context) => targetStyle.TextUnderlayY = MapNumber(property.children[0], context)},
                {"textunderlaydilate", (targetStyle, property, context) => targetStyle.TextUnderlayDilate = MapNumber(property.children[0], context)},
                {"textunderlaysoftness", (targetStyle, property, context) => targetStyle.TextUnderlaySoftness = MapNumber(property.children[0], context)},
                {"textunderlaytype", (targetStyle, property, context) => targetStyle.TextUnderlayType = MapEnum<UnderlayType>(property.children[0], context)},
                {"texttransform", (targetStyle, property, context) => targetStyle.TextTransform = MapEnum<TextTransform>(property.children[0], context)}, {
                    "textwhitespacemode", (targetStyle, property, context) => {
                        // couldn't find a generic version for merging a list of enum flags... just one that involves boxing: https://stackoverflow.com/questions/987607/c-flags-enum-generic-function-to-look-for-a-flag
                        WhitespaceMode result = MapEnum<WhitespaceMode>(property.children[0], context);
                        if (property.children.Count > 1) {
                            for (int index = 1; index < property.children.Count; index++) {
                                result |= MapEnum<WhitespaceMode>(property.children[index], context);
                            }
                        }

                        targetStyle.TextWhitespaceMode = result;
                    }
                },

                {"painter", (targetStyle, property, context) => targetStyle.Painter = MapPainter(property, context)},

                // Scrollbar
                {"scrollbar", (targetStyle, property, context) => targetStyle.Scrollbar = MapString(property.children[0], context)},
                {"scrollbarsize", (targetStyle, property, context) => targetStyle.ScrollbarSize = MapMeasurement(property.children[0], context)},
                {"scrollbarcolor", (targetStyle, property, context) => targetStyle.ScrollbarColor = MapColor(property, context)},

                // Shadows
            //    {"shadowtype", (targetStyle, property, context) => targetStyle.ShadowType = MapEnum<UnderlayType>(property.children[0], context)},
                {"shadowoffsetx", (targetStyle, property, context) => targetStyle.ShadowOffsetX = MapNumber(property.children[0], context)},
                {"shadowoffsety", (targetStyle, property, context) => targetStyle.ShadowOffsetY = MapNumber(property.children[0], context)},
                {"shadowsizex", (targetStyle, property, context) => targetStyle.ShadowSizeX = MapNumber(property.children[0], context)},
                {"shadowsizey", (targetStyle, property, context) => targetStyle.ShadowSizeY = MapNumber(property.children[0], context)},
                {"shadowintensity", (targetStyle, property, context) => targetStyle.ShadowIntensity = MapNumber(property.children[0], context)},
                {"shadowcolor", (targetStyle, property, context) => targetStyle.ShadowColor = MapColor(property.children[0], context)},
                {"shadowtint", (targetStyle, property, context) => targetStyle.ShadowTint = MapColor(property.children[0], context)},
                {"shadowopacity", (targetStyle, property, context) => targetStyle.ShadowOpacity = MapNumber(property.children[0], context)},
            };

        private static void MapLayoutFit(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            if (property.children.Count == 1) {
                var layoutFit = MapEnum<LayoutFit>(property.children[0], context);

                targetStyle.LayoutFitHorizontal = layoutFit;
                targetStyle.LayoutFitVertical = layoutFit;
            } else if (property.children.Count > 1) {
                var layoutFitX = MapEnum<LayoutFit>(property.children[0], context);
                var layoutFitY = MapEnum<LayoutFit>(property.children[1], context);

                targetStyle.LayoutFitHorizontal = layoutFitX;
                targetStyle.LayoutFitVertical = layoutFitY;
            }
        }

        private static void MapAlignmentX(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            if (property.children.size == 1) {
                // single value mode

                StyleASTNode value = context.GetValueForReference(property.children[0]);
                if (value is StyleIdentifierNode identifierNode) {
                    MapShorthandAlignmentX(targetStyle, context, identifierNode, value);
                }
                else {
                    OffsetMeasurement measurement = MapOffsetMeasurement(value, context);
                    targetStyle.AlignmentOriginX = measurement;
                    
                    if (measurement.unit == OffsetMeasurementUnit.Percent) {
                        targetStyle.AlignmentOffsetX = new OffsetMeasurement(-measurement.value, measurement.unit);
                    }
                }
            }
            else if (property.children.size == 2) {
                StyleASTNode value = context.GetValueForReference(property.children[0]);
                StyleASTNode value2 = context.GetValueForReference(property.children[1]);
                
                if (value is StyleIdentifierNode identifierNode1) {
                    MapShorthandAlignmentX(targetStyle, context, identifierNode1, value);
                    if (value2 is StyleIdentifierNode identifierNode2) {
                        targetStyle.AlignmentBehaviorX = MapEnum<AlignmentBehavior>(identifierNode2, context);
                    }
                    else {
                        targetStyle.AlignmentOffsetX = MapOffsetMeasurement(value2, context);
                    }
                }
                else {
                    OffsetMeasurement measurement = MapOffsetMeasurement(value, context);
                    targetStyle.AlignmentOriginX = measurement;
                    if (value2 is StyleIdentifierNode identifierNode2) {
                        targetStyle.AlignmentBehaviorX = MapEnum<AlignmentBehavior>(identifierNode2, context);
                        if (measurement.unit == OffsetMeasurementUnit.Percent) {
                            targetStyle.AlignmentOffsetX = new OffsetMeasurement(-measurement.value, measurement.unit);
                        }
                    }
                    else {
                        targetStyle.AlignmentOffsetX = MapOffsetMeasurement(value2, context);
                    }
                }
            }
            else if (property.children.size == 3) {
                targetStyle.AlignmentOriginX = MapOffsetMeasurement(context.GetValueForReference(property.children[0]), context);
                targetStyle.AlignmentOffsetX = MapOffsetMeasurement(context.GetValueForReference(property.children[1]), context);
                targetStyle.AlignmentDirectionX = MapEnum<AlignmentDirection>(context.GetValueForReference(property.children[2]), context);
            }
        }

        private static void MapAlignmentY(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            if (property.children.size == 1) {
                // single value mode
                StyleASTNode value = context.GetValueForReference(property.children[0]);
                if (value is StyleIdentifierNode identifierNode1) {
                    MapShorthandAlignmentY(targetStyle, context, identifierNode1, value);
                }
                else {
                    OffsetMeasurement measurement = MapOffsetMeasurement(value, context);
                    targetStyle.AlignmentOriginY = measurement;
                    if (measurement.unit == OffsetMeasurementUnit.Percent) {
                        targetStyle.AlignmentOffsetY = new OffsetMeasurement(-measurement.value, measurement.unit);
                    }
                }
            }
            else if (property.children.size == 2) {
                StyleASTNode value = context.GetValueForReference(property.children[0]);
                StyleASTNode value2 = context.GetValueForReference(property.children[1]);
                
                if (value is StyleIdentifierNode identifierNode1) {
                    MapShorthandAlignmentY(targetStyle, context, identifierNode1, value);
                    if (value2 is StyleIdentifierNode identifierNode2) {
                        targetStyle.AlignmentBehaviorY = MapEnum<AlignmentBehavior>(identifierNode2, context);
                    }
                    else {
                        targetStyle.AlignmentOffsetY = MapOffsetMeasurement(value2, context);
                    }
                }
                else {
                    OffsetMeasurement measurement = MapOffsetMeasurement(value, context);
                    targetStyle.AlignmentOriginY = measurement;
                    if (value2 is StyleIdentifierNode identifierNode2) {
                        targetStyle.AlignmentBehaviorY = MapEnum<AlignmentBehavior>(identifierNode2, context);
                        if (measurement.unit == OffsetMeasurementUnit.Percent) {
                            targetStyle.AlignmentOffsetY = new OffsetMeasurement(-measurement.value, measurement.unit);
                        }
                    }
                    else {
                        targetStyle.AlignmentOffsetY = MapOffsetMeasurement(value2, context);
                    }
                }
            }
            else if (property.children.size == 3) {
                targetStyle.AlignmentOriginY = MapOffsetMeasurement(context.GetValueForReference(property.children[0]), context);
                targetStyle.AlignmentOffsetY = MapOffsetMeasurement(context.GetValueForReference(property.children[1]), context);
                targetStyle.AlignmentDirectionY = MapEnum<AlignmentDirection>(context.GetValueForReference(property.children[2]), context);
            }
        }

        private static void MapShorthandAlignmentX(UIStyle targetStyle, StyleCompileContext context, StyleIdentifierNode identifierNode, StyleASTNode value) {
            switch (identifierNode.name.ToLower()) {
                case "start":
                    targetStyle.AlignmentOriginX = new OffsetMeasurement(0f, OffsetMeasurementUnit.Percent);
                    targetStyle.AlignmentOffsetX = new OffsetMeasurement(0f, OffsetMeasurementUnit.Percent);
                    break;
                case "center":
                    targetStyle.AlignmentOriginX = new OffsetMeasurement(0.5f, OffsetMeasurementUnit.Percent);
                    targetStyle.AlignmentOffsetX = new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent);
                    break;
                case "end":
                    targetStyle.AlignmentOriginX = new OffsetMeasurement(1f, OffsetMeasurementUnit.Percent);
                    targetStyle.AlignmentOffsetX = new OffsetMeasurement(-1f, OffsetMeasurementUnit.Percent);
                    break;
                default:
                    throw new CompileException(context.fileName, value, $"Invalid AlignX {value}. " +
                                                                        "Make sure you use one of the following keywords: start, center, end or provide an OffsetMeasurement.");
            }
        }

        private static void MapShorthandAlignmentY(UIStyle targetStyle, StyleCompileContext context, StyleIdentifierNode identifierNode, StyleASTNode value) {
            switch (identifierNode.name.ToLower()) {
                case "start":
                    targetStyle.AlignmentOriginY = new OffsetMeasurement(0);
                    targetStyle.AlignmentOffsetY = new OffsetMeasurement(0);
                    break;
                case "center":
                    targetStyle.AlignmentOriginY = new OffsetMeasurement(0.5f, OffsetMeasurementUnit.Percent);
                    targetStyle.AlignmentOffsetY = new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent);
                    break;
                case "end":
                    targetStyle.AlignmentOriginY = new OffsetMeasurement(1f, OffsetMeasurementUnit.Percent);
                    targetStyle.AlignmentOffsetY = new OffsetMeasurement(-1f, OffsetMeasurementUnit.Percent);
                    break;
                default:
                    throw new CompileException(context.fileName, value, $"Invalid AlignY {value}. " +
                                                                        "Make sure you use one of the following keywords: start, center, end or provide an OffsetMeasurement.");
            }
        }

        private static string MapPainter(PropertyNode property, StyleCompileContext context) {
            string customPainter = MapString(property.children[0], context);

            if (customPainter == "self" || customPainter == "none") {
                return customPainter;
            }

            if (string.IsNullOrEmpty(customPainter) || !Application.HasCustomPainter(customPainter)) {
                Debug.Log($"Could not find your custom painter {customPainter} in file {context.fileName}.");
            }

            return customPainter;
        }

        private static FontStyle MapTextFontStyle(PropertyNode property, StyleCompileContext context) {
            FontStyle style = FontStyle.Normal;

            foreach (StyleASTNode value in property.children) {
                StyleASTNode resolvedValue = context.GetValueForReference(value);
                switch (resolvedValue) {
                    case StyleIdentifierNode identifierNode:

                        string propertyValue = identifierNode.name.ToLower();

                        if (propertyValue.Contains("bold")) {
                            style |= Text.FontStyle.Bold;
                        }

                        if (propertyValue.Contains("italic")) {
                            style |= Text.FontStyle.Italic;
                        }

                        if (propertyValue.Contains("underline")) {
                            style |= Text.FontStyle.Underline;
                        }

                        if (propertyValue.Contains("strikethrough")) {
                            style |= Text.FontStyle.StrikeThrough;
                        }

                        break;
                    default:
                        throw new CompileException(context.fileName, value, $"Invalid TextFontStyle {value}. " +
                                                                            "Make sure you use one of those: bold, italic, underline or strikethrough.");
                }
            }

            return style;
        }


        private static void MapMaxSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.MaxWidth = x;
            targetStyle.MaxHeight = y;
        }

        private static void MapMinSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.MinWidth = x;
            targetStyle.MinHeight = y;
        }

        private static void MapPreferredSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.PreferredWidth = x;
            targetStyle.PreferredHeight = y;
        }

        private static CursorStyle MapCursor(PropertyNode property, StyleCompileContext context) {
            float hotSpotX = 0;
            float hotSpotY = 0;
            if (property.children.Count > 1) {
                hotSpotX = MapNumber(property.children[1], context);
                if (property.children.Count > 2) {
                    hotSpotY = MapNumber(property.children[2], context);
                }
                else {
                    hotSpotY = hotSpotX;
                }
            }

            return new CursorStyle(null, MapTexture(property.children[0], context), new Vector2(hotSpotX, hotSpotY));
        }

        private static GridItemPlacement MapGridItemPlacement(PropertyNode property, StyleCompileContext context) {
            StyleASTNode dereferencedValue = context.GetValueForReference(property.children[0]);

            switch (dereferencedValue.type) {
                case StyleASTNodeType.NumericLiteral:
                    int number = (int) MapNumber(dereferencedValue, context);
                    if (number < 0) {
                        return new GridItemPlacement(IntUtil.UnsetValue);
                    }

                    return new GridItemPlacement(number);

                case StyleASTNodeType.StringLiteral:
                    string placementName = MapString(property.children[0], context);
                    if (string.IsNullOrEmpty(placementName) || string.IsNullOrWhiteSpace(placementName) || placementName == ".") {
                        return new GridItemPlacement(IntUtil.UnsetValue);
                    }
                    else {
                        return new GridItemPlacement(placementName);
                    }
            }

            throw new CompileException(context.fileName, property, $"Had a hard time parsing that grid item placement: {property}.");
        }

        private static void MapTransformBehavior(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            TransformBehavior x = MapEnum<TransformBehavior>(property.children[0], context);
            TransformBehavior y = x;
            if (property.children.Count > 1) {
                y = MapEnum<TransformBehavior>(property.children[1], context);
            }

            targetStyle.TransformBehaviorX = x;
            targetStyle.TransformBehaviorY = y;
        }

        private static void MapTransformScale(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            float x = MapNumber(property.children[0], context);
            float y = x;
            if (property.children.Count > 1) {
                y = MapNumber(property.children[1], context);
            }

            targetStyle.TransformScaleX = x;
            targetStyle.TransformScaleY = y;
        }

        private static void MapTransformPivot(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength x = MapFixedLength(property.children[0], context);
            UIFixedLength y = x;
            if (property.children.Count > 1) {
                y = MapFixedLength(property.children[1], context);
            }

            targetStyle.TransformPivotX = x;
            targetStyle.TransformPivotY = y;
        }

        private static void MapTransformPosition(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            OffsetMeasurement x = MapOffsetMeasurement(property.children[0], context);
            OffsetMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapOffsetMeasurement(property.children[1], context);
            }

            targetStyle.TransformPositionX = x;
            targetStyle.TransformPositionY = y;
        }

        private static IReadOnlyList<GridTrackSize> MapGridLayoutTemplate(PropertyNode propertyNode, StyleCompileContext context) {
            LightList<GridTrackSize> gridTrackSizes = LightList<GridTrackSize>.Get();
            for (int index = 0; index < propertyNode.children.Count; index++) {
                StyleASTNode trackSize = propertyNode.children[index];
                gridTrackSizes.Add(MapGridTrackSize(trackSize, context));
            }

            return gridTrackSizes;
        }


        private static GridTrackSize MapGridTrackSize(StyleASTNode trackSize, StyleCompileContext context) {
            StyleASTNode dereferencedValue = context.GetValueForReference(trackSize);
            switch (dereferencedValue) {
                case StyleLiteralNode literalNode:
                    if (literalNode.type == StyleASTNodeType.StringLiteral && literalNode.rawValue.ToLower() == "auto") {
                        // todo revisit this default value and replace this with something else like 1mx? 
                        return GridTrackSize.Unset;
                    }
                    else if (literalNode.type == StyleASTNodeType.NumericLiteral && TryParseFloat(literalNode.rawValue, out float number)) {
                        return new GridTrackSize(number);
                    }

                    throw new CompileException(context.fileName, literalNode, $"Could not create a grid track size out of the value {literalNode}.");

                case MeasurementNode measurementNode:
                    GridTemplateUnit unit = MapGridTemplateUnit(measurementNode.unit, context);
                    float value = MapNumber(measurementNode.value, context);
                    return new GridTrackSize(value, unit);

                case StyleFunctionNode functionNode:

                    GridTrackSizeType trackSizeType;
                    GridTrackSize size = default;

                    switch (functionNode.identifier.ToLower()) {
                        case "repeat":
                            trackSizeType = GridTrackSizeType.Repeat;
                            if (functionNode.children.Count < 2) {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. Repeat must have at least two arguments.");
                            }

                            StyleASTNode firstChild = context.GetValueForReference(functionNode.children[0]);
                            if (firstChild is StyleLiteralNode literalNode) {
                                if (literalNode.type != StyleASTNodeType.NumericLiteral) {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }

                                float v = MapNumber(literalNode, context);
                                if (Mathf.Floor(v) != v || v < 1) {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }

                                size.value = v;
                                size.type = GridTrackSizeType.Repeat;
                            }
                            else if (firstChild is StyleIdentifierNode identifierNode) {
                                if (identifierNode.name == k_RepeatFill) {
                                    size.type = GridTrackSizeType.RepeatFill;
                                }
                                else if (identifierNode.name == k_RepeatFit) {
                                    size.type = GridTrackSizeType.RepeatFit;
                                }
                                else {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }
                            }
                            else {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                            }

                            size.pattern = MapGridTrackSizePattern(1, functionNode.children, context, true);

                            break;
                        case "minmax":
                            if (functionNode.children.Count != 2) {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. minmax() must have two arguments.");
                            }

                            size.type = GridTrackSizeType.MinMax;
                            size.pattern = MapGridTrackSizePattern(0, functionNode.children, context, false);

                            if (size.pattern.Length != 2) {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. minmax functions need to get exactly 2 arguments but I found {size.pattern.Length}.");
                            }

                            size.minUnit = size.pattern[0].unit;
                            size.minValue = size.pattern[0].value;

                            size.maxUnit = size.pattern[1].unit;
                            size.maxValue = size.pattern[1].value;
                            
                            break;
                        default:
                            throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. Expected a known track size function (repeat, grow, shrink) but all I got was {functionNode.identifier}");
                    }

                    return size;

                default:
                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}.");
            }
        }

        private static GridTrackSize[] MapGridTrackSizePattern(int startIndex, LightList<StyleASTNode> nodes, StyleCompileContext context, bool allowGrowOrShrink) {
            GridTrackSize[] retn = new GridTrackSize[nodes.Count - startIndex];

            for (int index = startIndex; index < nodes.Count; index++) {
                StyleASTNode argument = context.GetValueForReference(nodes[index]);
                GridTrackSize trackSize = MapGridTrackSize(argument, context);
                if (trackSize.type == GridTrackSizeType.Repeat) {
                    throw new CompileException(argument, "You cannot nest repeats.");
                }

                if (!allowGrowOrShrink && (trackSize.type == GridTrackSizeType.MinMax)) {
                    throw new CompileException(argument, "You cannot nest MinMaxes into each other.");
                }

                retn[index - startIndex] = trackSize;
            }

            return retn;
        }

        private static void MapBorders(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value1;
                targetStyle.BorderBottom = value1;
                targetStyle.BorderLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value1;
                targetStyle.BorderLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value3;
                targetStyle.BorderLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value3;
                targetStyle.BorderLeft = value4;
            }
        }

        private static void MapBorderRadius(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value1;
                targetStyle.BorderRadiusBottomRight = value1;
                targetStyle.BorderRadiusBottomLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value1;
                targetStyle.BorderRadiusBottomLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value3;
                targetStyle.BorderRadiusBottomLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value3;
                targetStyle.BorderRadiusBottomLeft = value4;
            }
        }

        private static string EnumValues(Type type) {
            return $"[{string.Join(", ", Enum.GetNames(type))}]";
        }

        private static void MapMargins(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            // We support all css notations here and accept, 1, 2, 3 and 4 values

            // - 1 value sets all 4 margins
            // - 2 values: first value sets top and bottom, second value sets left and right
            // - 3 values: first values sets top, 2nd sets left and right, 3rd sets bottom
            // - 4 values set all 4 margins from top to left, clockwise

            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value1;
                targetStyle.MarginBottom = value1;
                targetStyle.MarginLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value1;
                targetStyle.MarginLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value3;
                targetStyle.MarginLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value3;
                targetStyle.MarginLeft = value4;
            }
        }

        private static void MapBorderColors(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            Color value1 = MapColor(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value1;
                targetStyle.BorderColorBottom = value1;
                targetStyle.BorderColorLeft = value1;
            }
            else if (property.children.Count == 2) {
                Color value2 = MapColor(property.children[1], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value1;
                targetStyle.BorderColorLeft = value2;
            }
            else if (property.children.Count == 3) {
                Color value2 = MapColor(property.children[1], context);
                Color value3 = MapColor(property.children[2], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value3;
                targetStyle.BorderColorLeft = value2;
            }
            else if (property.children.Count > 3) {
                Color value2 = MapColor(property.children[1], context);
                Color value3 = MapColor(property.children[2], context);
                Color value4 = MapColor(property.children[3], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value3;
                targetStyle.BorderColorLeft = value4;
            }
        }

        private static void MapPaddings(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value1;
                targetStyle.PaddingBottom = value1;
                targetStyle.PaddingLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value1;
                targetStyle.PaddingLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value3;
                targetStyle.PaddingLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value3;
                targetStyle.PaddingLeft = value4;
            }
        }

        private static UIMeasurement MapMeasurement(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    UIMeasurementUnit unit = MapUnit(measurementNode.unit, context);
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        if (unit == UIMeasurementUnit.Percentage) {
                            measurementValue *= 0.01f;
                        }

                        return new UIMeasurement(measurementValue, unit);
                    }
                    else {
                        return new UIMeasurement(1f, unit);
                    }

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new UIMeasurement(literalValue);
                    }

                    break;
            }

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIFixedLength MapFixedLength(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        UIFixedUnit unit = MapFixedUnit(measurementNode.unit, context);
                        if (unit == UIFixedUnit.Percent) {
                            measurementValue /= 100f;
                        }

                        return new UIFixedLength(measurementValue, unit);
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new UIFixedLength(literalValue);
                    }

                    break;
            }

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIMeasurementUnit MapUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return UIMeasurementUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIMeasurementUnit.Pixel;

                case "pca":
                    return UIMeasurementUnit.ParentContentArea;

                case "psz":
                    return UIMeasurementUnit.ParentSize;

                case "em":
                    return UIMeasurementUnit.Em;

                case "cnt":
                case "content":
                    return UIMeasurementUnit.Content;

                case "vw":
                    return UIMeasurementUnit.ViewportWidth;

                case "vh":
                    return UIMeasurementUnit.ViewportHeight;

                case "%":
                    return UIMeasurementUnit.Percentage;

                case "mx":
                case "intrinsic":
                    return UIMeasurementUnit.IntrinsicPreferred;

                case "mn":
                case "intrinsic-min":
                    return UIMeasurementUnit.IntrinsicMinimum;

                case "fit-content":
                    return UIMeasurementUnit.FitContent;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, pca, pcz, em, cnt, vw, or vh instead (see UIMeasurementUnit). Will fall back to px.");

            return UIMeasurementUnit.Pixel;
        }

        private static UIFixedUnit MapAlignmentUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return UIFixedUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIFixedUnit.Pixel;
                case "%":
                    return UIFixedUnit.Percent;
                case "vh":
                    return UIFixedUnit.ViewportHeight;
                case "vw":
                    return UIFixedUnit.ViewportWidth;
                case "em":
                    return UIFixedUnit.Em;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, em, vw, vh or lh instead (see UIFixedUnit). Will fall back to px.");

            return UIFixedUnit.Pixel;
        }

        private static UIFixedUnit MapFixedUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return UIFixedUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIFixedUnit.Pixel;
                case "%":
                    return UIFixedUnit.Percent;
                case "vh":
                    return UIFixedUnit.ViewportHeight;
                case "vw":
                    return UIFixedUnit.ViewportWidth;
                case "em":
                    return UIFixedUnit.Em;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, em, vw, vh or lh instead (see UIFixedUnit). Will fall back to px.");

            return UIFixedUnit.Pixel;
        }

        private static GridTemplateUnit MapGridTemplateUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return GridTemplateUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return GridTemplateUnit.Pixel;
                case "mx":
                    return GridTemplateUnit.MaxContent;
                case "mn":
                    return GridTemplateUnit.MinContent;
                case "fr":
                    return GridTemplateUnit.FractionalRemaining;
                case "vw":
                    return GridTemplateUnit.ViewportWidth;
                case "vh":
                    return GridTemplateUnit.ViewportHeight;
                case "em":
                    return GridTemplateUnit.Em;
                case "pca":
                    return GridTemplateUnit.ParentContentArea;
                case "psz":
                    return GridTemplateUnit.ParentSize;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, mx, mn, em, vw, vh, fr, pca or psz instead (see GridTemplateUnit). Will fall back to px.");

            return GridTemplateUnit.Pixel;
        }

        private static OffsetMeasurement MapOffsetMeasurement(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        OffsetMeasurementUnit unit = MapTransformUnit(measurementNode.unit, context);
                        if (unit == OffsetMeasurementUnit.Percent) {
                            measurementValue *= 0.01f;
                        }

                        return new OffsetMeasurement(measurementValue, unit);
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new OffsetMeasurement(literalValue);
                    }

                    break;
            }

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static OffsetMeasurementUnit MapTransformUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return OffsetMeasurementUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return OffsetMeasurementUnit.Pixel;
                case "w":
                    return OffsetMeasurementUnit.ActualWidth;
                case "h":
                    return OffsetMeasurementUnit.ActualHeight;
                case "alw":
                    return OffsetMeasurementUnit.AllocatedWidth;
                case "alh":
                    return OffsetMeasurementUnit.AllocatedHeight;
                case "cw":
                    return OffsetMeasurementUnit.ContentWidth;
                case "ch":
                    return OffsetMeasurementUnit.ContentHeight;
                case "em":
                    return OffsetMeasurementUnit.Em;
                case "caw":
                    return OffsetMeasurementUnit.ContentAreaWidth;
                case "cah":
                    return OffsetMeasurementUnit.ContentAreaHeight;
                case "vw":
                    return OffsetMeasurementUnit.ViewportWidth;
                case "vh":
                    return OffsetMeasurementUnit.ViewportHeight;
                case "pw":
                    return OffsetMeasurementUnit.ParentWidth;
                case "ph":
                    return OffsetMeasurementUnit.ParentHeight;
                case "pcaw":
                    return OffsetMeasurementUnit.ParentContentAreaWidth;
                case "pcah":
                    return OffsetMeasurementUnit.ParentContentAreaHeight;
                case "sw":
                    return OffsetMeasurementUnit.ScreenWidth;
                case "sh":
                    return OffsetMeasurementUnit.ScreenHeight;
                case "%":
                    return OffsetMeasurementUnit.Percent;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, w, h, alw, alh, cw, ch, em, caw, cah, vw, vh, pw, ph, pcaw, pcah, sw, or sh instead (see TransformUnit). Will fall back to px.");

            return OffsetMeasurementUnit.Pixel;
        }

        private static void MapOverflows(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            Overflow overflowX = MapEnum<Overflow>(property.children[0], context);
            Overflow overflowY = overflowX;

            if (property.children.Count == 2) {
                overflowY = MapEnum<Overflow>(property.children[1], context);
            }

            // should we check for more than 2 values and log a warning?

            targetStyle.OverflowX = overflowX;
            targetStyle.OverflowY = overflowY;
        }

        private static Texture2D MapTexture(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            switch (node) {
                case UrlNode urlNode:
                    AssetInfo assetInfo = TransformUrlNode(urlNode, context);
                    if (assetInfo.SpriteName != null) {
                        throw new CompileException(urlNode, "SpriteAtlas access is coming soon!");
                    }

                    return context.application.ResourceManager.GetTexture(assetInfo.Path);
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(context.fileName, node, $"Expected url(path/to/texture) but found {node}.");
        }

        private static FontAsset MapFont(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            switch (node) {
                case UrlNode urlNode:
                    AssetInfo assetInfo = TransformUrlNode(urlNode, context);
                    if (assetInfo.SpriteName != null) {
                        throw new CompileException(urlNode, "SpriteAtlas access is coming soon!");
                    }

                    return context.application.ResourceManager.GetFont(assetInfo.Path);
                // return ResourceManager.GetFont(TransformUrlNode(urlNode, context));
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(context.fileName, node, $"Expected url(path/to/font) but found {node}.");
        }

        private static AssetInfo TransformUrlNode(UrlNode urlNode, StyleCompileContext context) {
            StyleASTNode url = context.GetValueForReference(urlNode.url);
            StyleASTNode spriteNameNode = context.GetValueForReference(urlNode.spriteName);
            string spriteName = null;
            if (spriteNameNode != null && spriteNameNode is StyleLiteralNode stringNode) {
                spriteName = stringNode.rawValue;
            }

            if (url.type == StyleASTNodeType.Identifier) {
                return new AssetInfo {
                    Path = ((StyleIdentifierNode) url).name,
                    SpriteName = spriteName
                };
            }

            if (url.type == StyleASTNodeType.StringLiteral) {
                return new AssetInfo {
                    Path = ((StyleLiteralNode) url).rawValue,
                    SpriteName = spriteName
                };
            }

            throw new CompileException(url, "Invalid url value.");
        }


        private static Color MapColor(PropertyNode property, StyleCompileContext context) {
            AssertSingleValue(property.children, context);
            return MapColor(property.children[0], context);
        }

        private static Color MapColor(StyleASTNode colorStyleAstNode, StyleCompileContext context) {
            var styleAstNode = context.GetValueForReference(colorStyleAstNode);
            switch (styleAstNode) {
                case StyleIdentifierNode identifierNode:
                    return ParseColor(identifierNode, context);

                case ColorNode colorNode: return colorNode.color;
                case RgbaNode rgbaNode: return MapRbgaNodeToColor(rgbaNode, context);
                case RgbNode rgbNode: return MapRgbNodeToColor(rgbNode, context);
                default:
                    throw new CompileException(context.fileName, styleAstNode, "Unsupported color value.");
            }
        }

        private static Color32 ParseColor(StyleIdentifierNode node, StyleCompileContext context) {
            switch (node.name.ToLower()) {
                case "clear": return new Color32(0, 0, 0, 0);
                case "transparent": return new Color32(0, 0, 0, 0);
                case "black": return new Color32(0, 0, 0, 255);
                case "indianred": return new Color32(205, 92, 92, 255);
                case "lightcoral": return new Color32(240, 128, 128, 255);
                case "salmon": return new Color32(250, 128, 114, 255);
                case "darksalmon": return new Color32(233, 150, 122, 255);
                case "lightsalmon": return new Color32(255, 160, 122, 255);
                case "crimson": return new Color32(220, 20, 60, 255);
                case "red": return new Color32(255, 0, 0, 255);
                case "firebrick": return new Color32(178, 34, 34, 255);
                case "darkred": return new Color32(139, 0, 0, 255);
                case "pink": return new Color32(255, 192, 203, 255);
                case "lightpink": return new Color32(255, 182, 193, 255);
                case "hotpink": return new Color32(255, 105, 180, 255);
                case "deeppink": return new Color32(255, 20, 147, 255);
                case "mediumvioletred": return new Color32(199, 21, 133, 255);
                case "palevioletred": return new Color32(219, 112, 147, 255);
                case "coral": return new Color32(255, 127, 80, 255);
                case "tomato": return new Color32(255, 99, 71, 255);
                case "orangered": return new Color32(255, 69, 0, 255);
                case "darkorange": return new Color32(255, 140, 0, 255);
                case "orange": return new Color32(255, 165, 0, 255);
                case "gold": return new Color32(255, 215, 0, 255);
                case "yellow": return new Color32(255, 255, 0, 255);
                case "lightyellow": return new Color32(255, 255, 224, 255);
                case "lemonchiffon": return new Color32(255, 250, 205, 255);
                case "lightgoldenrodyellow": return new Color32(250, 250, 210, 255);
                case "papayawhip": return new Color32(255, 239, 213, 255);
                case "moccasin": return new Color32(255, 228, 181, 255);
                case "peachpuff": return new Color32(255, 218, 185, 255);
                case "palegoldenrod": return new Color32(238, 232, 170, 255);
                case "khaki": return new Color32(240, 230, 140, 255);
                case "darkkhaki": return new Color32(189, 183, 107, 255);
                case "lavender": return new Color32(230, 230, 250, 255);
                case "thistle": return new Color32(216, 191, 216, 255);
                case "plum": return new Color32(221, 160, 221, 255);
                case "violet": return new Color32(238, 130, 238, 255);
                case "orchid": return new Color32(218, 112, 214, 255);
                case "fuchsia": return new Color32(255, 0, 255, 255);
                case "magenta": return new Color32(255, 0, 255, 255);
                case "mediumorchid": return new Color32(186, 85, 211, 255);
                case "mediumpurple": return new Color32(147, 112, 219, 255);
                case "blueviolet": return new Color32(138, 43, 226, 255);
                case "darkviolet": return new Color32(148, 0, 211, 255);
                case "darkorchid": return new Color32(153, 50, 204, 255);
                case "darkmagenta": return new Color32(139, 0, 139, 255);
                case "purple": return new Color32(128, 0, 128, 255);
                case "rebeccapurple": return new Color32(102, 51, 153, 255);
                case "indigo": return new Color32(75, 0, 130, 255);
                case "mediumslateblue": return new Color32(123, 104, 238, 255);
                case "slateblue": return new Color32(106, 90, 205, 255);
                case "darkslateblue": return new Color32(72, 61, 139, 255);
                case "greenyellow": return new Color32(173, 255, 47, 255);
                case "chartreuse": return new Color32(127, 255, 0, 255);
                case "lawngreen": return new Color32(124, 252, 0, 255);
                case "lime": return new Color32(0, 255, 0, 255);
                case "limegreen": return new Color32(50, 205, 50, 255);
                case "palegreen": return new Color32(152, 251, 152, 255);
                case "lightgreen": return new Color32(144, 238, 144, 255);
                case "mediumspringgreen": return new Color32(0, 250, 154, 255);
                case "springgreen": return new Color32(0, 255, 127, 255);
                case "mediumseagreen": return new Color32(60, 179, 113, 255);
                case "seagreen": return new Color32(46, 139, 87, 255);
                case "forestgreen": return new Color32(34, 139, 34, 255);
                case "green": return new Color32(0, 128, 0, 255);
                case "darkgreen": return new Color32(0, 100, 0, 255);
                case "yellowgreen": return new Color32(154, 205, 50, 255);
                case "olivedrab": return new Color32(107, 142, 35, 255);
                case "olive": return new Color32(128, 128, 0, 255);
                case "darkolivegreen": return new Color32(85, 107, 47, 255);
                case "mediumaquamarine": return new Color32(102, 205, 170, 255);
                case "darkseagreen": return new Color32(143, 188, 143, 255);
                case "lightseagreen": return new Color32(32, 178, 170, 255);
                case "darkcyan": return new Color32(0, 139, 139, 255);
                case "teal": return new Color32(0, 128, 128, 255);
                case "aqua": return new Color32(0, 255, 255, 255);
                case "cyan": return new Color32(0, 255, 255, 255);
                case "lightcyan": return new Color32(224, 255, 255, 255);
                case "paleturquoise": return new Color32(175, 238, 238, 255);
                case "aquamarine": return new Color32(127, 255, 212, 255);
                case "turquoise": return new Color32(64, 224, 208, 255);
                case "mediumturquoise": return new Color32(72, 209, 204, 255);
                case "darkturquoise": return new Color32(0, 206, 209, 255);
                case "cadetblue": return new Color32(95, 158, 160, 255);
                case "steelblue": return new Color32(70, 130, 180, 255);
                case "lightsteelblue": return new Color32(176, 196, 222, 255);
                case "powderblue": return new Color32(176, 224, 230, 255);
                case "lightblue": return new Color32(173, 216, 230, 255);
                case "skyblue": return new Color32(135, 206, 235, 255);
                case "lightskyblue": return new Color32(135, 206, 250, 255);
                case "deepskyblue": return new Color32(0, 191, 255, 255);
                case "dodgerblue": return new Color32(30, 144, 255, 255);
                case "cornflowerblue": return new Color32(100, 149, 237, 255);
                case "royalblue": return new Color32(65, 105, 225, 255);
                case "blue": return new Color32(0, 0, 255, 255);
                case "mediumblue": return new Color32(0, 0, 205, 255);
                case "darkblue": return new Color32(0, 0, 139, 255);
                case "navy": return new Color32(0, 0, 128, 255);
                case "midnightblue": return new Color32(25, 25, 112, 255);
                case "cornsilk": return new Color32(255, 248, 220, 255);
                case "blanchedalmond": return new Color32(255, 235, 205, 255);
                case "bisque": return new Color32(255, 228, 196, 255);
                case "navajowhite": return new Color32(255, 222, 173, 255);
                case "wheat": return new Color32(245, 222, 179, 255);
                case "burlywood": return new Color32(222, 184, 135, 255);
                case "tan": return new Color32(210, 180, 140, 255);
                case "rosybrown": return new Color32(188, 143, 143, 255);
                case "sandybrown": return new Color32(244, 164, 96, 255);
                case "goldenrod": return new Color32(218, 165, 32, 255);
                case "darkgoldenrod": return new Color32(184, 134, 11, 255);
                case "peru": return new Color32(205, 133, 63, 255);
                case "chocolate": return new Color32(210, 105, 30, 255);
                case "saddlebrown": return new Color32(139, 69, 19, 255);
                case "sienna": return new Color32(160, 82, 45, 255);
                case "brown": return new Color32(165, 42, 42, 255);
                case "maroon": return new Color32(128, 0, 0, 255);
                case "white": return new Color32(255, 255, 255, 255);
                case "snow": return new Color32(255, 250, 250, 255);
                case "honeydew": return new Color32(240, 255, 240, 255);
                case "mintcream": return new Color32(245, 255, 250, 255);
                case "azure": return new Color32(240, 255, 255, 255);
                case "aliceblue": return new Color32(240, 248, 255, 255);
                case "ghostwhite": return new Color32(248, 248, 255, 255);
                case "whitesmoke": return new Color32(245, 245, 245, 255);
                case "seashell": return new Color32(255, 245, 238, 255);
                case "beige": return new Color32(245, 245, 220, 255);
                case "oldlace": return new Color32(253, 245, 230, 255);
                case "floralwhite": return new Color32(255, 250, 240, 255);
                case "ivory": return new Color32(255, 255, 240, 255);
                case "antiquewhite": return new Color32(250, 235, 215, 255);
                case "linen": return new Color32(250, 240, 230, 255);
                case "lavenderblush": return new Color32(255, 240, 245, 255);
                case "mistyrose": return new Color32(255, 228, 225, 255);
                case "gainsboro": return new Color32(220, 220, 220, 255);
                case "lightgray": return new Color32(211, 211, 211, 255);
                case "lightgrey": return new Color32(211, 211, 211, 255);
                case "silver": return new Color32(192, 192, 192, 255);
                case "darkgray": return new Color32(169, 169, 169, 255);
                case "darkgrey": return new Color32(169, 169, 169, 255);
                case "gray": return new Color32(128, 128, 128, 255);
                case "grey": return new Color32(128, 128, 128, 255);
                case "dimgray": return new Color32(105, 105, 105, 255);
                case "dimgrey": return new Color32(105, 105, 105, 255);
                case "lightslategray": return new Color32(119, 136, 153, 255);
                case "lightslategrey": return new Color32(119, 136, 153, 255);
                case "slategray": return new Color32(112, 128, 144, 255);
                case "slategrey": return new Color32(112, 128, 144, 255);
                case "darkslategray": return new Color32(47, 79, 79, 255);
                case "darkslategrey": return new Color32(47, 79, 79, 255);
            }

            throw new CompileException(context.fileName, node, $"Unable to map color name: {node.name} to a color");
        }

        private static Color MapRbgaNodeToColor(RgbaNode rgbaNode, StyleCompileContext context) {
            byte red = (byte) MapNumber(rgbaNode.red, context);
            byte green = (byte) MapNumber(rgbaNode.green, context);
            byte blue = (byte) MapNumber(rgbaNode.blue, context);
            byte alpha = (byte) MapNumber(rgbaNode.alpha, context);

            return new Color32(red, green, blue, alpha);
        }

        private static Color MapRgbNodeToColor(RgbNode rgbaNode, StyleCompileContext context) {
            byte red = (byte) MapNumber(rgbaNode.red, context);
            byte green = (byte) MapNumber(rgbaNode.green, context);
            byte blue = (byte) MapNumber(rgbaNode.blue, context);

            return new Color32(red, green, blue, 255);
        }

        internal static float MapRelativeValue(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);

            switch (node) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        if (measurementNode.unit.value == "%") {
                            measurementValue *= 0.01f;
                        }
                        else {
                            throw new CompileException(context.fileName, node, $"This property only accepts % as a unit but you used a {measurementNode.unit.value}");
                        }

                        return measurementValue;
                    }

                    break;
                case StyleIdentifierNode identifierNode: {
                    switch (identifierNode.name.ToLower()) {
                        case "start": return 0f;
                        case "center": return 0.5f;
                        case "end": return 1f;
                        default: throw new CompileException(context.fileName, node, $"Expected a [start|center|end] but all I got was this lousy {node}");
                    }
                }
                case StyleLiteralNode literalNode: {
                    if (literalNode.type == StyleASTNodeType.NumericLiteral) {
                        if (TryParseFloat(((StyleLiteralNode) node).rawValue, out float number)) {
                            return number;
                        }
                    }

                    break;
                } 
            }

            throw new CompileException(context.fileName, node, $"Expected a numeric value but all I got was this lousy {node}");
        }

        internal static float MapNumber(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            if (node is StyleIdentifierNode identifierNode) {
                if (TryParseFloat(identifierNode.name, out float number)) {
                    return number;
                }
            }

            if (node.type == StyleASTNodeType.NumericLiteral) {
                if (TryParseFloat(((StyleLiteralNode) node).rawValue, out float number)) {
                    return number;
                }
            }

            throw new CompileException(context.fileName, node, $"Expected a numeric value but all I got was this lousy {node}");
        }

        private static string MapString(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                return identifierNode.name;
            }

            if (node is StyleLiteralNode literalNode && literalNode.type == StyleASTNodeType.StringLiteral) {
                return literalNode.rawValue;
            }

            throw new CompileException(context.fileName, node, $"Expected a string value but all I got was this lousy {node}");
        }

        public static void MapProperty(UIStyle targetStyle, PropertyNode node, StyleCompileContext context) {
            string propertyName = node.identifier;
            LightList<StyleASTNode> propertyValues = node.children;

            if (propertyValues.Count == 0) {
                throw new CompileException(node, "Property does not have a value.");
            }

            string propertyKey = propertyName.ToLower();

            mappers.TryGetValue(propertyKey, out Action<UIStyle, PropertyNode, StyleCompileContext> action);
            action?.Invoke(targetStyle, node, context);
            if (action == null) Debug.LogWarning($"{propertyKey} at column {node.column} line {node.line} in file {context.fileName} is an unknown style property.");
        }

        internal static T MapEnum<T>(StyleASTNode node, StyleCompileContext context) where T : struct {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                if (Enum.TryParse(identifierNode.name, true, out T thing)) {
                    return thing;
                }
            }

            throw new CompileException(context.fileName, node, $"Expected a proper {typeof(T).Name} value, which must be one of " +
                                                               $"{EnumValues(typeof(T))} and your " +
                                                               $"value {node} does not match any of them.");
        }

        private static bool TryParseFloat(string input, out float result) {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static void AssertSingleValue(LightList<StyleASTNode> propertyValues, StyleCompileContext context) {
            if (propertyValues.Count > 1) {
                throw new CompileException(context.fileName, propertyValues[1], "Found too many values.");
            }
        }

        public struct AssetInfo {

            public string Path;
            public string SpriteName;

        }

    }

}