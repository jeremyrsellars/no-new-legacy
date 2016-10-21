class Ruhoh
  module Converter
    module Markdown
      def self.extensions
        ['.md', '.markdown']
      end
      def self.convert(content)
        require 'redcarpet'
        markdown = Redcarpet::Markdown.new(MarkdownRenderer.new(:with_toc_data => true),
          :no_intra_emphasis => true,
          :tables => true,
          :space_after_headers => true,
          :disable_indented_code_blocks => true,
          :autolink => true, 
          :strikethrough => true, 
          :fenced_code_blocks => true, 
          :lax_spacing => true,
          :superscript => true,
          :underline => true,
        )
        markdown.render(content)
      end
    end
  end
end
