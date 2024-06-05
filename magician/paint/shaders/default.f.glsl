#version 330 core

in vec4 rgba2;

out vec4 out_col;

void main()
{
    out_col = vec4(rgba2.x, rgba2.y, rgba2.z, rgba2.w);
}