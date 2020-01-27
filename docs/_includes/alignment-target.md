The AlignmentTarget properties define the box, which the AlignmentOrigin will be relative to.
Here's a list currently supported targets:

| value             | description                                                                                                                          |
|:------------------|:-------------------------------------------------------------------------------------------------------------------------------------|
| LayoutBox         | the default; target the [layout box](/docs/layout/#layoutbox-and-allocated-size) that has been assigned to the element by its parent |
| Parent            | Target the parent's layout box.                                                                                                      |
| ParentContentArea | Target the parent's [content box](/docs/layout/#box-model).                                                                          |
| View              | Target the view. Does not affect parent transforms!                                                                                  |
| Screen            | Target the screen. Does not affect parent transforms!                                                                                |
