require 'redcarpet'

class Ruhoh
  class MarkdownRenderer < Redcarpet::Render::HTML
    def link(link, title, alt_text)
      if(link =~ /^(?:https?|ftp):/i)
        "<a href=\"#{link}\"  target=\"_blank\" title=\"#{title}\" rel=\"noopener\" class=\"external\">#{alt_text}</a>"
      else
        "<a href=\"#{link}\" title=\"#{title}\">#{alt_text}</a>"
      end
    end
    def autolink(link, link_type)
      friendly = link.gsub(/^(?:https?|ftp):\/\/(?:www\.)?|\/$/i, "")
      if(link =~ /^(?:https?|ftp):/i)
        "<a href=\"#{link}\"  target=\"_blank\" rel=\"noopener\" class=\"external\">#{friendly}</a>"
      else
        "<a href=\"#{link}\">#{friendly}</a>"
      end
    end
  end
end