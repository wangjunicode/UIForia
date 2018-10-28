
    // clip rect is in texture space with origin top left
    // texcoord origin is bottom left, invert to match
    float2 p = float2(IN.texcoord.x, 1 - IN.texcoord.y);
    float4 clipRect = m_ClipRect;
    return m_FillColor;
    //clip((step(clipRect.xy, p) * step(p, clipRect.zw)) - 0.01);
    
    float blur = 1.414;
    float2 size = m_Size;
    float width = size.x;
    float height = size.y;
    float2 halfSize = size * 0.5;
    float2 texcoordMinusHalf = IN.texcoord - 0.5;

    float2 centerOffset = texcoordMinusHalf * size;
    
    FillSettings fillSettings;
    fillSettings.fillTexture = m_FillTexture;
    fillSettings.fillColor1 = m_FillColor;
    fillSettings.fillColor2 = m_FillColor2;
    fillSettings.fillRotation = m_FillRotation;
    fillSettings.fillOffset = m_FillOffset;
    fillSettings.fillScale = m_FillScale;
    fillSettings.gradientAxis = m_GradientAxis;
    fillSettings.gradientStart = m_GradientStart;
    fillSettings.gridSize = m_GridSize;
    fillSettings.lineSize = m_LineSize;
    
    fixed4 fillColor = fill(texcoordMinusHalf, size, fillSettings);
    
#if defined(UIFORIA_USE_BORDER)    
    int left = step(0.5, IN.texcoord.x);
    int top = step(0.5, IN.texcoord.y);
    
    int indexer = 0;
    indexer += and(top, left) * 1;
    indexer += and(1 - top, 1 - left) * 2;
    indexer += and(1 - top, left) * 3;
    
    // subtract the AA blur from the outline so sizes stay mostly 
    // correct and outlines don't look too thick when scaled down.
    float outlineSize = max(0, m_BorderSize[indexer] - blur);

    // some of these are constants, maybe move to constant buffer or vertex shader
    float halfMinDimension = min(halfSize.x, halfSize.y);
    float outerBlur = max(min(blur, halfMinDimension - outlineSize), 0);
    float innerBlur = max(min(outerBlur, halfMinDimension - outerBlur - outlineSize), 0);
                    
    float roundness = m_BorderRadius[indexer];
    float radius = min(halfMinDimension, roundness);
    float2 extents = halfSize - radius;
    float2 delta = abs(centerOffset) - extents;
    
    // first component is distance to closest side when not in a corner circle,
    // second is distance to the rounded part when in a corner circle
    float dist = radius - (min(max(delta.x, delta.y), 0) + length(max(delta, 0)));
    
    float outline = outerBlur + outlineSize;
    innerBlur += outline;
    fixed4 color = outline_fill_blend(dist, fillColor, m_BorderColor, outerBlur, outline, innerBlur);
#else

    float2 delta = abs(centerOffset) - halfSize;
    float dist = -(min(max(delta.x, delta.y), 0) + length(max(delta, 0)));
    fixed4 color = fill_blend(dist, fillColor, 0 /*blur*/); //using blur here makes a 1px gap on the edges for some reason
    
#endif

   // clip(color.a - 0.001);
    
    color.rgb *= color.a;
    return fillSettings.fillColor1;
