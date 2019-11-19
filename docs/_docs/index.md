---
layout: page
title: Documentation
permalink: /docs/
---

# Documentation

Welcome to the {{ site.title }} Documentation pages! Here you can quickly jump to a 
particular page.

<div class="section-index row">
    <hr class="panel-line col-12">
    {% assign posts = site.docs | where: 'showInMain', true | sort: 'order' %}
    {% for post in posts %}
    <div class="entry col-4">
        <h5><a href="{{ post.url | prepend: site.baseurl }}">{{ post.title }}</a></h5>
        <p>{{ post.description }}</p>
    </div>{% endfor %}
</div>
