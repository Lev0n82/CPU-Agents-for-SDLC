import { useEffect } from 'react';

interface SEOProps {
  title: string;
  description: string;
  keywords: string;
  ogType?: string;
  canonical?: string;
}

export function SEO({ title, description, keywords, ogType = 'website', canonical }: SEOProps) {
  useEffect(() => {
    // Update title
    document.title = `${title} | CPU Agents for SDLC`;

    // Update or create meta tags
    const updateMetaTag = (name: string, content: string, property = false) => {
      const attribute = property ? 'property' : 'name';
      let element = document.querySelector(`meta[${attribute}="${name}"]`);
      
      if (!element) {
        element = document.createElement('meta');
        element.setAttribute(attribute, name);
        document.head.appendChild(element);
      }
      
      element.setAttribute('content', content);
    };

    // Standard meta tags
    updateMetaTag('description', description);
    updateMetaTag('keywords', keywords);

    // Open Graph tags
    updateMetaTag('og:title', `${title} | CPU Agents for SDLC`, true);
    updateMetaTag('og:description', description, true);
    updateMetaTag('og:type', ogType, true);

    // Twitter Card tags
    updateMetaTag('twitter:title', `${title} | CPU Agents for SDLC`);
    updateMetaTag('twitter:description', description);

    // Canonical URL
    if (canonical) {
      let linkElement = document.querySelector('link[rel="canonical"]');
      if (!linkElement) {
        linkElement = document.createElement('link');
        linkElement.setAttribute('rel', 'canonical');
        document.head.appendChild(linkElement);
      }
      linkElement.setAttribute('href', canonical);
    }
  }, [title, description, keywords, ogType, canonical]);

  return null;
}
