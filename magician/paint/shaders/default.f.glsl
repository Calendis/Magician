#version 330 core

in vec4 rgba;

out vec4 out_col;

void main()
{
    out_col = vec4(rgba.x, rgba.y, rgba.z, rgba.w);
}