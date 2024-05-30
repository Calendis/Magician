#version 330 core

in vec3 pos2;
in vec4 rgba2;

out vec4 out_col;

void main()
{
    out_col = vec4(1-rgba2.x, 1-rgba2.y, 1-rgba2.z, rgba2.w);
}